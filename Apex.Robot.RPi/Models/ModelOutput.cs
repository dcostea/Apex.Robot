
using Microsoft.ML.Data;

namespace Apex.Robot.RPi.Models;
public class ModelOutput
{
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; }

    [ColumnName("Score")]
    public float[] Score { get; set; }
}
