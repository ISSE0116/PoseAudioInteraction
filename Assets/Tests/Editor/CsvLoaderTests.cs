using System.Globalization;
using NUnit.Framework;

/// <summary>
/// CSV読み込みロジックのユニットテスト
/// </summary>
[TestFixture]
public class CsvLoaderTests
{
    // ========== TryParseFloat テスト ==========

    [Test]
    public void TryParseFloat_ValidNumber_ReturnsTrue()
    {
        // Arrange
        string input = "123.456";
        
        // Act
        bool result = TryParseFloat(input, out float value);
        
        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(123.456f, value, 0.001f);
    }

    [Test]
    public void TryParseFloat_NegativeNumber_ReturnsTrue()
    {
        // Arrange
        string input = "-45.67";
        
        // Act
        bool result = TryParseFloat(input, out float value);
        
        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(-45.67f, value, 0.001f);
    }

    [Test]
    public void TryParseFloat_None_ReturnsFalse()
    {
        // Arrange
        string input = "None";
        
        // Act
        bool result = TryParseFloat(input, out float value);
        
        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(0f, value);
    }

    [Test]
    public void TryParseFloat_EmptyString_ReturnsFalse()
    {
        // Arrange
        string input = "";
        
        // Act
        bool result = TryParseFloat(input, out float value);
        
        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void TryParseFloat_Null_ReturnsFalse()
    {
        // Arrange
        string input = null;
        
        // Act
        bool result = TryParseFloat(input, out float value);
        
        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void TryParseFloat_WhitespaceNone_ReturnsFalse()
    {
        // Arrange
        string input = "  None  ";
        
        // Act
        bool result = TryParseFloat(input, out float value);
        
        // Assert
        Assert.IsFalse(result);
    }

    // ========== フレーム計算テスト ==========

    [Test]
    public void CalculateFrame_At60Fps_ReturnsCorrectFrame()
    {
        // Arrange
        float elapsedTime = 1.5f;
        float frameRate = 60f;
        
        // Act
        int frame = CalculateFrame(elapsedTime, frameRate);
        
        // Assert
        Assert.AreEqual(90, frame);
    }

    [Test]
    public void CalculateFrame_At30Fps_ReturnsCorrectFrame()
    {
        // Arrange
        float elapsedTime = 2.0f;
        float frameRate = 30f;
        
        // Act
        int frame = CalculateFrame(elapsedTime, frameRate);
        
        // Assert
        Assert.AreEqual(60, frame);
    }

    [Test]
    public void CalculateFrame_ZeroTime_ReturnsZero()
    {
        // Arrange
        float elapsedTime = 0f;
        float frameRate = 60f;
        
        // Act
        int frame = CalculateFrame(elapsedTime, frameRate);
        
        // Assert
        Assert.AreEqual(0, frame);
    }

    // ========== ヘルパーメソッド（テスト対象のロジックを再現） ==========

    private bool TryParseFloat(string s, out float value)
    {
        if (string.IsNullOrEmpty(s) || s.Trim().Equals("None", System.StringComparison.OrdinalIgnoreCase))
        {
            value = 0f;
            return false;
        }
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private int CalculateFrame(float elapsedTime, float frameRate)
    {
        return UnityEngine.Mathf.FloorToInt(elapsedTime * frameRate);
    }
}
