using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mahjong.Model;
using Mahjong.Logic;

public class PointInfoTest : MonoBehaviour
{
    [Test]
    public void TestRealYakumanHandScore()
    {
        var yakuList = new List<YakuValue>();
        yakuList.Add(new YakuValue { Name = "Daisuushi", Value = 2, Type = YakuType.Yakuman });
        var pointInfo = new PointInfo(
            20, yakuList, false, false, 0, 0, 0, 0
            );
        Assert.AreEqual(32000, pointInfo.calculateTotalPoints(false, false, 4));
        Assert.AreEqual(48000, pointInfo.calculateTotalPoints(true, false, 4));
        Assert.AreEqual(32000, pointInfo.calculateTotalPoints(false, true, 4));
        Assert.AreEqual(48000, pointInfo.calculateTotalPoints(true, true, 4));

        Assert.AreEqual(16000, pointInfo.calculateTsumoDealerPayment());
        Assert.AreEqual(16000, pointInfo.calculateTsumoNonDealerPayment(true));
        Assert.AreEqual(8000, pointInfo.calculateTsumoNonDealerPayment(false));
    }

    [TestCase(14, 48000, 32000, 16000, 8000)]
    [TestCase(13, 48000, 32000, 16000, 8000)]
    [TestCase(12, 36000, 24000, 12000, 6000)]
    [TestCase(11, 36000, 24000, 12000, 6000)]
    [TestCase(10, 24000, 16000, 8000, 4000)]
    [TestCase(9, 24000, 16000, 8000, 4000)]
    [TestCase(8, 24000, 16000, 8000, 4000)]
    [TestCase(7, 18000, 12000, 6000, 3000)]
    [TestCase(6, 18000, 12000, 6000, 3000)]
    [TestCase(5, 12000, 8000, 4000, 2000)]
    public void TestFiveHanAndAboveHandScore(int numHan, int dealerTotalPoints, int nonDealerTotalPoints, int tsumoDealerPayment, int tsumoNonDealerPayment)
    {
        var yakuList = new List<YakuValue>();
        yakuList.Add(new YakuValue { Name = "Gaming", Value = numHan, Type = YakuType.Normal });
        var pointInfo = new PointInfo(
            20, yakuList, false, false, 0, 0, 0, 0
            );
        Assert.AreEqual(nonDealerTotalPoints, pointInfo.calculateTotalPoints(false, false, 4));
        Assert.AreEqual(dealerTotalPoints, pointInfo.calculateTotalPoints(true, false, 4));

        Assert.AreEqual(tsumoDealerPayment, pointInfo.calculateTsumoDealerPayment());
        Assert.AreEqual(tsumoNonDealerPayment, pointInfo.calculateTsumoNonDealerPayment(false));
    }

    [TestCase(4, 20, 7700, 5200, 7800, 5200, 2600, 1300)]
    [TestCase(4, 25, 9600, 6400, 9600, 6400, 3200, 1600)]
    [TestCase(4, 30, 11600, 7700, 11700, 7900, 3900, 2000)]
    [TestCase(4, 40, 12000, 8000, 12000, 8000, 4000, 2000)]
    [TestCase(3, 20, 3900, 2600, 3900, 2700, 1300, 700)]
    [TestCase(3, 25, 4800, 3200, 4800, 3200, 1600, 800)]
    [TestCase(3, 30, 5800, 3900, 6000, 4000, 2000, 1000)]
    [TestCase(3, 40, 7700, 5200, 7800, 5200, 2600, 1300)]
    [TestCase(3, 50, 9600, 6400, 9600, 6400, 3200, 1600)]
    [TestCase(2, 20, 2000, 1300, 2100, 1500, 700, 400)]
    [TestCase(2, 25, 2400, 1600, 2400, 1600, 800, 400)]
    [TestCase(2, 30, 2900, 2000, 3000, 2000, 1000, 500)]
    [TestCase(2, 40, 3900, 2600, 3900, 2700, 1300, 700)]
    [TestCase(2, 50, 4800, 3200, 4800, 3200, 1600, 800)]
    [TestCase(1, 30, 1500, 1000, 1500, 1100, 500, 300)]
    [TestCase(1, 40, 2000, 1300, 2100, 1500, 700, 400)]
    [TestCase(1, 50, 2400, 1600, 2400, 1600, 800, 400)]
    public void TestFourHanAndBelowHandScore(int numHan, int numFu, int dealerTotalPoints, int nonDealerTotalPoints, int dealerTsumoTotalPoints, int nonDealerTsumoTotalPoints, int tsumoDealerPayment, int tsumoNonDealerPayment)
    {
        var yakuList = new List<YakuValue>();
        yakuList.Add(new YakuValue { Name = "Gaming", Value = numHan, Type = YakuType.Normal });
        var pointInfo = new PointInfo(
            numFu, yakuList, false, false, 0, 0, 0, 0
            );
        Assert.AreEqual(nonDealerTotalPoints, pointInfo.calculateTotalPoints(false, false, 4));
        Assert.AreEqual(dealerTotalPoints, pointInfo.calculateTotalPoints(true, false, 4));
        Assert.AreEqual(nonDealerTsumoTotalPoints, pointInfo.calculateTotalPoints(false, true, 4));
        Assert.AreEqual(dealerTsumoTotalPoints, pointInfo.calculateTotalPoints(true, true, 4));

        Assert.AreEqual(tsumoDealerPayment, pointInfo.calculateTsumoDealerPayment());
        Assert.AreEqual(tsumoNonDealerPayment, pointInfo.calculateTsumoNonDealerPayment(false));
    }

    [TestCase(14, 48000, 32000, 48000, 32000, 20000, 12000, 24000)]
    [TestCase(13, 48000, 32000, 48000, 32000, 20000, 12000, 24000)]
    [TestCase(12, 36000, 24000, 36000, 24000, 16000, 8000, 18000)]
    [TestCase(11, 36000, 24000, 36000, 24000, 16000, 8000, 18000)]
    [TestCase(10, 24000, 16000, 24000, 16000, 10000, 6000, 12000)]
    [TestCase(9, 24000, 16000, 24000, 16000, 10000, 6000, 12000)]
    [TestCase(8, 24000, 16000, 24000, 16000, 10000, 6000, 12000)]
    [TestCase(7, 18000, 12000, 18000, 12000, 8000, 4000, 9000)]
    [TestCase(6, 18000, 12000, 18000, 12000, 8000, 4000, 9000)]
    [TestCase(5, 12000, 8000, 12000, 8000, 5000, 3000, 6000)]
    [TestCase(4, 12000, 8000, 12000, 8000, 5000, 3000, 6000)]
    [TestCase(3, 6000, 4000, 6000, 4000, 3000, 1000, 3000)]
    [TestCase(2, 3000, 2000, 4000, 2000, 1000, 1000, 2000)]
    [TestCase(1, 2000, 1000, 2000, 2000, 1000, 1000, 1000)]
    public void TestSuperBingoHandScore(int numHan, int dealerTotalPoints, int nonDealerTotalPoints, int dealerTsumoTotalPoints, int nonDealerTsumoTotalPoints, int tsumoDealerPayment, int tsumoNonDealerPaymentAsNonDealer, int tsumoNonDealerPaymentAsDealer)
    {
        var yakuList = new List<YakuValue>();
        yakuList.Add(new YakuValue { Name = "Gaming", Value = numHan, Type = YakuType.Normal });
        var pointInfo = new PointInfo(
            20, yakuList, false, true, 0, 0, 0, 0
            );
        Assert.AreEqual(nonDealerTotalPoints, pointInfo.calculateTotalPoints(false, false, 3));
        Assert.AreEqual(dealerTotalPoints, pointInfo.calculateTotalPoints(true, false, 3));
        Assert.AreEqual(nonDealerTsumoTotalPoints, pointInfo.calculateTotalPoints(false, true, 3));
        Assert.AreEqual(dealerTsumoTotalPoints, pointInfo.calculateTotalPoints(true, true, 3));

        Assert.AreEqual(tsumoDealerPayment, pointInfo.calculateTsumoDealerPayment());
        Assert.AreEqual(tsumoNonDealerPaymentAsNonDealer, pointInfo.calculateTsumoNonDealerPayment(false));
        Assert.AreEqual(tsumoNonDealerPaymentAsDealer, pointInfo.calculateTsumoNonDealerPayment(true));
    }
}
