
using Microsoft.ML.Data;

namespace Apex.Robot.RPi.Models;
public class ModelOutput
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    public float Score { get; set; }

    // The probability calculated by calibrating the score of having true as the label.
    public float Probability { get; set; }
}
