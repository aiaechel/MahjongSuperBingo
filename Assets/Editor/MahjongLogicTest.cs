using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Mahjong.Model;
using Mahjong.Logic;
using UnityEditor;
using System;

public class MahjongLogicTest
{
    /*
     * Tests to write
     * 
     * - getPointInfo: Set up a couple of tests for different settings to pass in a bunch of stuff to check fu and han count
     * - HasWin: Test if hand + melds + some tile is a complete hand without checking yaku
     * - WinningTiles: Get winning tiles for current hand without checking yaku
     * - DiscardForReady: Get what tiles I can discard for tenpai without checking yaku
     * - TestRichi: can I riichi?
     * - GetKongs: Daiminkan
     * - GetSelfKongs: Ankan
     * - GetAddKongs: Shouminkan
     * - GetRichiKongs: Can I Kan a certain triplet during riichi
     * - GetPongs: Can I pon a tile
     * - GetChows: Can I chii a tile
     * - TestDiscardZhenting: Am I furiten from my discards
     * - GetDoraTile: Get the dora tile for a given dora indicator
     */

    // For a given hand and status, get the resulting PointInfo.
    [TestCase("2334457m444p678s", new String[] {}, "7m", HandStatus.Menqing, 1, 40, TestName = "Tanyao")]
    [TestCase("23344577m456p68s", new String[] {}, "7s", HandStatus.Menqing, 1, 40, TestName = "Tanyao Kanchan")]
    [TestCase("123456789m2348p", new String[] {}, "8p", HandStatus.Menqing, 2, 40, TestName = "Closed Itsu")]
    [TestCase("123456789m8p", new String[] { "2'34p" }, "8p", HandStatus.Nothing, 1, 30, TestName = "Open Itsu")]
    [TestCase("1223377m789p678s", new String[] {}, "4m", HandStatus.Menqing, 1, 30, TestName = "Pinfu Ron")]
    [TestCase("1223377m789p678s", new String[] {}, "4m", MenzenTsumo, 2, 20, TestName = "Pinfu Tsumo")]
    [TestCase("1122377m789p678s", new String[] {}, "3m", HandStatus.Menqing, 1, 40, TestName = "Iipeikou Penchan")]
    public void TestGetPointInfo(String handTileString, String[] meldStrings, String winningTileString, HandStatus handStatus, int expectedHan, int? expectedFu)
    {
        var handTiles = convertNotationToTiles(handTileString);
        var melds = new Meld[meldStrings.Length];
        for (int i = 0; i < meldStrings.Length; i++) {
            melds[i] = getMeldFromString(meldStrings[i]);
        }
        var winningTile = convertNotationToTiles(winningTileString)[0];
        var roundStatus = new RoundStatus {
            TotalPlayer = 4
        };
        var gameSetting = new GameSetting();
        var result = MahjongLogic.GetPointInfo(handTiles.ToArray(), melds, winningTile, handStatus, roundStatus, gameSetting, false);
        Assert.AreEqual(expectedHan, result.TotalFan);
        if (expectedFu != null) {
            Assert.AreEqual(expectedFu, result.Fu);
        }
    }

    [TestCase("19m19p19s1234567z", "1m", TestName = "Kokushi 13 sided wait")]
    [TestCase("111333555m888p1z", "1z", TestName = "Suuankou Tanki")]
    public void TestGetPointInfoYakuman(String handTileString, String winningTileString) {
        var handTiles = convertNotationToTiles(handTileString);
        var melds = new Meld[0];
        var winningTile = convertNotationToTiles(winningTileString)[0];
        var handStatus = HandStatus.Nothing;
        var roundStatus = new RoundStatus {
            TotalPlayer = 4
        };
        var gameSetting = new GameSetting();
        var result = MahjongLogic.GetPointInfo(handTiles.ToArray(), melds, winningTile, handStatus, roundStatus, gameSetting, false);
        Assert.IsTrue(result.IsYakuman);
    }

    [Test]
    public void TestKokushiThirteenSideWait()
    {
        var handTiles = convertNotationToTiles("19m19p19s1234567z");
        var winningTiles = MahjongLogic.WinningTiles(handTiles, new List<Meld>());
        Assert.AreEqual(13, winningTiles.Count);
    }

    [Test]
    public void Test7PairWait()
    {
        var handTiles = convertNotationToTiles("11m99p199s225566z");
        var winningTiles = MahjongLogic.WinningTiles(handTiles, new List<Meld>());
        Assert.AreEqual(1, winningTiles.Count);
        Assert.AreEqual(new Tile(Suit.S, 1), winningTiles[0]);

        handTiles = convertNotationToTiles("2334455667788s");
        winningTiles = MahjongLogic.WinningTiles(handTiles, new List<Meld>());
        Assert.AreEqual(3, winningTiles.Count);
        Assert.AreEqual(new Tile(Suit.S, 2), winningTiles[0]);
        Assert.AreEqual(new Tile(Suit.S, 5), winningTiles[1]);
        Assert.AreEqual(new Tile(Suit.S, 8), winningTiles[2]);
    }

    [Test]
    public static void TestDoraIndicator()
    {
        var allTiles = MahjongConstants.TwoPlayerTiles;
        var indicator = new Tile(Suit.Z, 2);
        Assert.AreEqual(new Tile(Suit.Z, 3), MahjongLogic.GetDoraTile(indicator, allTiles));
        indicator = new Tile(Suit.Z, 7);
        Assert.AreEqual(new Tile(Suit.Z, 5), MahjongLogic.GetDoraTile(indicator, allTiles));
        indicator = new Tile(Suit.Z, 4);
        Assert.AreEqual(new Tile(Suit.Z, 1), MahjongLogic.GetDoraTile(indicator, allTiles));
        indicator = new Tile(Suit.S, 7);
        Assert.AreEqual(new Tile(Suit.S, 8), MahjongLogic.GetDoraTile(indicator, allTiles));
        indicator = new Tile(Suit.S, 9);
        Assert.AreEqual(new Tile(Suit.S, 1), MahjongLogic.GetDoraTile(indicator, allTiles));
        indicator = new Tile(Suit.S, 1);
        Assert.AreEqual(new Tile(Suit.S, 2), MahjongLogic.GetDoraTile(indicator, allTiles));
        indicator = new Tile(Suit.M, 1);
        Assert.AreEqual(new Tile(Suit.M, 9), MahjongLogic.GetDoraTile(indicator, allTiles));
        indicator = new Tile(Suit.M, 4);
        Assert.AreEqual(new Tile(Suit.M, 4), MahjongLogic.GetDoraTile(indicator, allTiles));
        indicator = new Tile(Suit.M, 9);
        Assert.AreEqual(new Tile(Suit.M, 1), MahjongLogic.GetDoraTile(indicator, allTiles));
    }

    public static void TestPongs()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 5, true)
        };
        var result = MahjongLogic.GetPongs(handTiles, new Tile(Suit.M, 4), MeldSide.Opposite);
        Debug.Log($"Melds: {string.Join(",", result)}");
        result = MahjongLogic.GetPongs(handTiles, new Tile(Suit.M, 5), MeldSide.Opposite);
        Debug.Log($"Melds: {string.Join(",", result)}");
        result = MahjongLogic.GetPongs(handTiles, new Tile(Suit.M, 5, true), MeldSide.Opposite);
        Debug.Log($"Melds: {string.Join(",", result)}");
        handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 5)
        };
        result = MahjongLogic.GetPongs(handTiles, new Tile(Suit.M, 5, true), MeldSide.Opposite);
        Debug.Log($"Melds: {string.Join(",", result)}");
    }

    public static void TestChows()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 5, true), new Tile(Suit.M, 6)
        };
        var result = MahjongLogic.GetChows(handTiles, new Tile(Suit.M, 4), MeldSide.Left);
        Debug.Log($"Melds: {string.Join(",", result)}");
        handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 6)
        };
        result = MahjongLogic.GetChows(handTiles, new Tile(Suit.M, 5, true), MeldSide.Opposite);
        Debug.Log($"Melds: {string.Join(",", result)}");
    }

    public static void TestCombinations()
    {
        var list = new List<int> {
            1, 2, 3, 4, 5, 6, 7, 8
        };
        var result = MahjongLogic.Combination(list, 1);
        Debug.Log($"Total results: {result.Count}");
        for (int i = 0; i < result.Count; i++)
        {
            Debug.Log($"{i}: {string.Join(",", result[i])}");
        }
    }

    public static void TestDiscard()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5, true)
        };
        var dict = MahjongLogic.DiscardForReady(handTiles, new Tile(Suit.Z, 1));
        foreach (var item in dict)
        {
            Debug.Log($"{item.Key}, {string.Join(",", item.Value)}");
        }
    }

    public static void TestRichiKongs()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.M, 3), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5, true), new Tile(Suit.M, 5)
        };
        var kongs = MahjongLogic.GetRichiKongs(handTiles, new Tile(Suit.M, 5));
        Debug.Log($"Kongs: {string.Join(",", kongs)}");
        handTiles = new List<Tile> {
            new Tile(Suit.M, 3), new Tile(Suit.M, 3), new Tile(Suit.M, 6), new Tile(Suit.M, 6),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5, true), new Tile(Suit.M, 5)
        };
        kongs = MahjongLogic.GetRichiKongs(handTiles, new Tile(Suit.M, 5));
        Debug.Log($"Kongs: {string.Join(",", kongs)}");
        kongs = MahjongLogic.GetRichiKongs(handTiles, new Tile(Suit.M, 3));
        Debug.Log($"Kongs: {string.Join(",", kongs)}");
    }

    // Convert something like 123m123s123456p22z to tiles
    private static List<Tile> convertNotationToTiles(String notation) {
        var result = new List<Tile>();
        var sublist = new List<int>();
        for (int i = 0; i < notation.Length; i++) {
            if (Char.IsNumber(notation[i])) {
                sublist.Add((int) Char.GetNumericValue(notation[i]));
            } else {
                Suit suit;
                switch(notation[i]) {
                    case 'm':
                        suit = Suit.M;
                        break;
                    case 'p':
                        suit = Suit.P;
                        break;
                    case 's':
                        suit = Suit.S;
                        break;
                    case 'z':
                        suit = Suit.Z;
                        break;
                    default: 
                        continue;
                }
                foreach (int num in sublist) {
                    result.Add(new Tile(suit, num));
                }
                sublist.Clear();
            }
        }
        if (sublist.Count != 0) {
            throw new ArgumentException("Missing a suit");
        }
        return result;
    }

    private Meld getMeldFromString(String meldString) {
        var revealed = meldString.Contains("'") || meldString.Contains('"');
        return new Meld(revealed, convertNotationToTiles(meldString).ToArray());
    }

    private const HandStatus MenzenTsumo = HandStatus.Menqing | HandStatus.Tsumo;
}
