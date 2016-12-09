#include "pch.h"
#include "Dht11.h"
#include <ppltasks.h>

using namespace concurrency;
using namespace std;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::System::Threading;
using namespace Windows::Devices::Gpio;
using namespace IoTHelpers::Runtime;

_Use_decl_annotations_

Dht11::Dht11(GpioPin^ pin)
{
    // Use InputPullUp if supported, otherwise fall back to Input (floating)
    this->inputDriveMode =
        pin->IsDriveModeSupported(GpioPinDriveMode::InputPullUp) ?
        GpioPinDriveMode::InputPullUp : GpioPinDriveMode::Input;

    pin->SetDriveMode(this->inputDriveMode);
    this->pin = pin;
}

Dht11::~Dht11()
{
	this->pin = nullptr;
}

IAsyncOperation<Dht11Reading>^ Dht11::GetReadingAsync()
{
	return this->GetReadingAsync(DEFAULT_MAX_RETRIES);
}

IAsyncOperation<Dht11Reading>^ Dht11::GetReadingAsync(int maxRetries)
{
	return create_async([this, maxRetries]
	{
        Dht11Reading returnValue;
		int i = 0;

		for (i = 0; i < maxRetries; i++)
		{
			returnValue = this->GetReadingInternal();
			if (returnValue.IsValid)
				break;
		}

		returnValue.RetryCount = i;
		return returnValue;
	});
}

Dht11Reading Dht11::GetReadingInternal()
{
    Dht11Reading returnValue;

	// Create a buffer for the 40-bit reading
	std::bitset<40> bits;

	// Query the performance counter frequency
	// to calculate the correct timing
	LARGE_INTEGER qpf;
	QueryPerformanceFrequency(&qpf);

    // This is the threshold used to determine whether a bit is a '0' or a '1'.
    // A '0' has a pulse time of 76 microseconds, while a '1' has a
    // pulse time of 120 microseconds. 110 is chosen as a reasonable threshold.
    // We convert the value to QPF units for later use.
	const unsigned int oneThreshold = static_cast<unsigned int>(110LL * qpf.QuadPart / 1000000LL);

    // Latch low value onto pin
	this->pin->Write(GpioPinValue::Low);

    // Set pin as output
	this->pin->SetDriveMode(GpioPinDriveMode::Output);

    // Wait for at least 18 ms
	Sleep(SAMPLE_HOLD_LOW_MILLIS);

    // Set pin back to input
	this->pin->SetDriveMode(this->inputDriveMode);

	GpioPinValue previousValue = this->pin->Read();

    // catch the first rising edge
	const ULONG initialRisingEdgeTimeoutMillis = 1;
	ULONGLONG endTickCount = GetTickCount64() + initialRisingEdgeTimeoutMillis;
	while (true)
	{
		if (GetTickCount64() > endTickCount)
		{
			returnValue.TimedOut = true;
			return returnValue;
		}

		GpioPinValue value = this->pin->Read();
		if (value != previousValue)
		{
			/// rising edgue?
			if (value == GpioPinValue::High)
				break;

			previousValue = value;
		}
	}

	LARGE_INTEGER prevTime = { 0 };

	const ULONG sampleTimeoutMillis = 10;
	endTickCount = GetTickCount64() + sampleTimeoutMillis;

    // capture every falling edge until all bits are received or
    // timeout occurs
	for (unsigned int i = 0; i < (bits.size() + 1);)
	{
		if (GetTickCount64() > endTickCount)
		{
			returnValue.TimedOut = true;
			return returnValue;
		}

		GpioPinValue value = this->pin->Read();
		if ((previousValue == GpioPinValue::High) && (value == GpioPinValue::Low))
		{
            // A falling edge was detected
			LARGE_INTEGER now;
			QueryPerformanceCounter(&now);

			if (i != 0)
			{
				unsigned int difference = static_cast<unsigned int>(now.QuadPart - prevTime.QuadPart);
				bits[bits.size() - i] = difference > oneThreshold;
			}

			prevTime = now;
			++i;
		}

		previousValue = value;
	}

	returnValue = this->GetValuesFromReading(bits);
	return returnValue;
}

Dht11Reading Dht11::GetValuesFromReading(std::bitset<40> bits)
{
    Dht11Reading returnValue;

	unsigned long long value = bits.to_ullong();

	unsigned int checksum =
		((value >> 32) & 0xff) +
		((value >> 24) & 0xff) +
		((value >> 16) & 0xff) +
		((value >> 8) & 0xff);

	returnValue.IsValid = (checksum & 0xff) == (value & 0xff);

	if (returnValue.IsValid)
	{
		returnValue.Humidity = ((value >> 32) & 0xff) + ((value >> 24) & 0xff) / 10.0;
		returnValue.Temperature = ((value >> 16) & 0xff) + ((value >> 8) & 0xff) / 10.0;
	}

	return returnValue;
}
