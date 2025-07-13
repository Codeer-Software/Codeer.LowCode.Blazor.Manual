using LowCodeSamples.Client.Shared.Samples.MobileSensor;

namespace LowCodeApp
{
    public class MauiGeolocationService : IGeolocationService
    {
        public async Task<GeoLocation?> GetLocationAsync()
        {
            try
            {
                var loc = await Geolocation.Default.GetLocationAsync();
                if (loc == null)
                {
                    return null;
                }
                return new GeoLocation(loc.Latitude, loc.Longitude);
            }
            catch
            {
                return null;
            }
        }
    }

    public class MauiAccelerometerService : IAccelerometerService
    {
        public event Action<double, double, double>? ReadingChanged;

        public void Start(SensorUpdateSpeed speed)
        {
            try
            {
                var mauiSpeed = speed switch
                {
                    SensorUpdateSpeed.UI => SensorSpeed.UI,
                    SensorUpdateSpeed.Game => SensorSpeed.Game,
                    SensorUpdateSpeed.Fastest => SensorSpeed.Fastest,
                    _ => SensorSpeed.Default,
                };
                Accelerometer.Default.ReadingChanged += OnReading;
                Accelerometer.Default.Start(mauiSpeed);
            }
            catch { }
        }

        public void Stop()
        {
            Accelerometer.Default.ReadingChanged -= OnReading;
            Accelerometer.Default.Stop();
        }

        void OnReading(object? s, AccelerometerChangedEventArgs e)
            => ReadingChanged?.Invoke(
                 e.Reading.Acceleration.X,
                 e.Reading.Acceleration.Y,
                 e.Reading.Acceleration.Z);
    }
}
