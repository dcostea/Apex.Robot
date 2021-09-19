
using Microsoft.ML.Data;

namespace Apex.Robot.RPi.Models;
public class ModelInput
{
    [LoadColumn(0)]
    [ColumnName(@"Temperature")]
    public float Temperature { get; set; }

    [LoadColumn(1)]
    [ColumnName(@"Humidity")]
    public float Humidity { get; set; }

    [LoadColumn(2)]
    [ColumnName(@"Infrared")]
    public float Infrared { get; set; }

    [LoadColumn(3)]
    [ColumnName(@"Distance")]
    public float Distance { get; set; }

    [LoadColumn(4)]
    [ColumnName(@"CreatedAt")]
    public string CreatedAt { get; set; }

    [LoadColumn(5)]
    [ColumnName(@"IsAlarm")]
    public bool IsAlarm { get; set; }

    public override string ToString()
    {
        return $"Temperature: {Temperature} Humidity: {Humidity} Infrared: {Infrared} Distance: {Distance} CreatedAt: {CreatedAt} IsAlarm: {IsAlarm}";
    }
}
