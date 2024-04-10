using System.Text.Json;
using System.Text.Json.Serialization;
using VoronoiProject.Models;

namespace VoronoiProject.Services
{
    public class PointConverter : JsonConverter<Point>
    {
        public override Point? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
        {
            // Json doesn't like infinities. Make them maximized
            var stringX = double.IsInfinity(value.X) ? double.MaxValue * Math.Sign(value.X) : value.X;
            var stringY = double.IsInfinity(value.Y) ? double.MaxValue * Math.Sign(value.Y) : value.Y;

            writer.WriteRawValue($"{{\"x\": {stringX}, \"y\": {stringY}, \"voronoiPoints\": []}}");
        }
    }
}
