#pragma once

using namespace Windows::Foundation;
using namespace Windows::Devices::Gpio;

#define SAMPLE_HOLD_LOW_MILLIS 18
#define DEFAULT_MAX_RETRIES 20

namespace IoTHelpers
{
	namespace Runtime
	{
		public value struct Dht11Reading
		{
			bool TimedOut;
			bool IsValid;
			int RetryCount;
			double Temperature;
			double Humidity;
		};

        public ref class Dht11 sealed
		{
			public:
				Dht11(GpioPin^ pin);
				virtual ~Dht11();

				IAsyncOperation<Dht11Reading>^ GetReadingAsync();
				IAsyncOperation<Dht11Reading>^ GetReadingAsync(int maxRetries);

			private:
				GpioPin^ pin;
				GpioPinDriveMode inputDriveMode;

                Dht11Reading GetReadingInternal();
                Dht11Reading Dht11::GetValuesFromReading(std::bitset<40> bits);
		};
	}
}