
namespace LowCodeSamples.Client.Shared.Samples.MobileSensor
{
    public enum SensorUpdateSpeed
    {
        Default,
        UI,
        Game,
        Fastest
    }

    public interface IAccelerometerService
    {
        void Start(SensorUpdateSpeed speed);
        void Stop();
        event Action<double, double, double>? ReadingChanged;
    }

    public interface IGeolocationService
    {
        Task<GeoLocation?> GetLocationAsync();
    }

    public record GeoLocation(double Latitude, double Longitude);
}
