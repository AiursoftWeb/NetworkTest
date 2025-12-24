using Aiursoft.NetworkTest.Models;

namespace Aiursoft.NetworkTest.Tests;

[TestClass]
public class NATTestResultTests
{
    [TestMethod]
    public void Score_CombinesBaseScoreAndUPnPBonus()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.FullCone,
            BaseScore = 80.0,
            UPnPBonus = 15.0
        };

        // Act
        var score = result.Score;

        // Assert
        Assert.AreEqual(95.0, score, "Score should be base + bonus");
    }

    [TestMethod]
    public void Score_WithNoBonus_ReturnsOnlyBaseScore()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.Symmetric,
            BaseScore = 30.0,
            UPnPBonus = 0.0
        };

        // Act
        var score = result.Score;

        // Assert
        Assert.AreEqual(30.0, score, "Score should equal base score when no bonus");
    }

    [TestMethod]
    public void NATTypeDescription_OpenInternet_ReturnsCorrectDescription()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.OpenInternet,
            BaseScore = 100.0
        };

        // Act
        var description = result.NATTypeDescription;

        // Assert
        Assert.AreEqual("Open Internet (No NAT)", description);
    }

    [TestMethod]
    public void NATTypeDescription_FullCone_ReturnsCorrectDescription()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.FullCone,
            BaseScore = 90.0
        };

        // Act
        var description = result.NATTypeDescription;

        // Assert
        Assert.AreEqual("Full Cone NAT", description);
    }

    [TestMethod]
    public void NATTypeDescription_RestrictedCone_ReturnsCorrectDescription()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.RestrictedCone,
            BaseScore = 70.0
        };

        // Act
        var description = result.NATTypeDescription;

        // Assert
        Assert.AreEqual("Restricted Cone NAT", description);
    }

    [TestMethod]
    public void NATTypeDescription_PortRestrictedCone_ReturnsCorrectDescription()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.PortRestrictedCone,
            BaseScore = 50.0
        };

        // Act
        var description = result.NATTypeDescription;

        // Assert
        Assert.AreEqual("Port Restricted Cone NAT", description);
    }

    [TestMethod]
    public void NATTypeDescription_Symmetric_ReturnsCorrectDescription()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.Symmetric,
            BaseScore = 0.0
        };

        // Act
        var description = result.NATTypeDescription;

        // Assert
        Assert.AreEqual("Symmetric NAT", description);
    }

    [TestMethod]
    public void NATTypeDescription_UDPBlocked_ReturnsCorrectDescription()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.UDPBlocked,
            BaseScore = 0.0
        };

        // Act
        var description = result.NATTypeDescription;

        // Assert
        Assert.AreEqual("UDP Blocked", description);
    }

    [TestMethod]
    public void NATTypeDescription_Unknown_ReturnsCorrectDescription()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.Unknown,
            BaseScore = 0.0
        };

        // Act
        var description = result.NATTypeDescription;

        // Assert
        Assert.AreEqual("Unknown", description);
    }

    [TestMethod]
    public void P2PCapability_OpenInternet_ReturnsPerfect()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.OpenInternet,
            BaseScore = 100.0
        };

        // Act
        var capability = result.P2PCapability;

        // Assert
        Assert.AreEqual("Perfect", capability);
    }

    [TestMethod]
    public void P2PCapability_FullCone_ReturnsExcellent()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.FullCone,
            BaseScore = 90.0
        };

        // Act
        var capability = result.P2PCapability;

        // Assert
        Assert.AreEqual("Excellent", capability);
    }

    [TestMethod]
    public void P2PCapability_RestrictedCone_ReturnsGood()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.RestrictedCone,
            BaseScore = 70.0
        };

        // Act
        var capability = result.P2PCapability;

        // Assert
        Assert.AreEqual("Good", capability);
    }

    [TestMethod]
    public void P2PCapability_PortRestrictedCone_WithUPnP_ReturnsGoodUPnPAssisted()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.PortRestrictedCone,
            BaseScore = 50.0,
            UPnPAvailable = true
        };

        // Act
        var capability = result.P2PCapability;

        // Assert
        Assert.AreEqual("Good (UPnP assisted)", capability);
    }

    [TestMethod]
    public void P2PCapability_PortRestrictedCone_WithoutUPnP_ReturnsFair()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.PortRestrictedCone,
            BaseScore = 50.0,
            UPnPAvailable = false
        };

        // Act
        var capability = result.P2PCapability;

        // Assert
        Assert.AreEqual("Fair", capability);
    }

    [TestMethod]
    public void P2PCapability_Symmetric_ReturnsPoor()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.Symmetric,
            BaseScore = 0.0
        };

        // Act
        var capability = result.P2PCapability;

        // Assert
        Assert.AreEqual("Poor (P2P difficult)", capability);
    }

    [TestMethod]
    public void P2PCapability_UDPBlocked_ReturnsBlocked()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.UDPBlocked,
            BaseScore = 0.0
        };

        // Act
        var capability = result.P2PCapability;

        // Assert
        Assert.AreEqual("Blocked", capability);
    }

    [TestMethod]
    public void P2PCapability_Unknown_ReturnsUnknown()
    {
        // Arrange
        var result = new NATTestResult
        {
            NATType = NATType.Unknown,
            BaseScore = 0.0
        };

        // Act
        var capability = result.P2PCapability;

        // Assert
        Assert.AreEqual("Unknown", capability);
    }

    [TestMethod]
    public void AllProperties_CanBeInitialized()
    {
        // Arrange & Act
        var result = new NATTestResult
        {
            NATType = NATType.FullCone,
            LocalIP = "192.168.1.100",
            MappedPublicIP = "203.0.113.5",
            MappedPublicPort = 12345,
            BehindNAT = true,
            UPnPAvailable = true,
            UPnPTestedPort = 8080,
            BaseScore = 90.0,
            UPnPBonus = 10.0,
            TestDetails = new List<string> { "Test 1", "Test 2" }
        };

        // Assert
        Assert.AreEqual(NATType.FullCone, result.NATType);
        Assert.AreEqual("192.168.1.100", result.LocalIP);
        Assert.AreEqual("203.0.113.5", result.MappedPublicIP);
        Assert.AreEqual(12345, result.MappedPublicPort);
        Assert.IsTrue(result.BehindNAT);
        Assert.IsTrue(result.UPnPAvailable);
        Assert.AreEqual(8080, result.UPnPTestedPort);
        Assert.AreEqual(90.0, result.BaseScore);
        Assert.AreEqual(10.0, result.UPnPBonus);
        Assert.AreEqual(100.0, result.Score);
        Assert.AreEqual(2, result.TestDetails.Count);
    }

    [TestMethod]
    public void TestDetails_DefaultsToEmptyList()
    {
        // Arrange & Act
        var result = new NATTestResult
        {
            NATType = NATType.Unknown,
            BaseScore = 0.0
        };

        // Assert
        Assert.IsNotNull(result.TestDetails);
        Assert.AreEqual(0, result.TestDetails.Count);
    }

    [TestMethod]
    public void BehindNAT_CanBeFalse()
    {
        // Arrange & Act
        var result = new NATTestResult
        {
            NATType = NATType.OpenInternet,
            BaseScore = 100.0,
            BehindNAT = false
        };

        // Assert
        Assert.IsFalse(result.BehindNAT);
    }
}
