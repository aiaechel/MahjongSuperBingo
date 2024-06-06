using System;
using System.Collections.Generic;
using System.Linq;
using GamePlay.Server.Model;
using Mahjong.Logic;

namespace Mahjong.Model
{
    [Serializable]
    public struct PointInfo : IComparable<PointInfo>
    {
        public int Fu { get; }
        private int Fan;
        private YakuValue[] Yakus;
        public bool IsYakuman { get; }
        public bool IsQTJ { get; }
        public bool IsSuperBingo { get; }
        public int Dora { get; }
        public int UraDora { get; }
        public int RedDora { get; }
        public int BeiDora { get; }
        public int Doras { get; }

        public PointInfo(int fu, IList<YakuValue> yakuValues, bool 青天井, bool isSuperBingo, int dora, int uraDora, int redDora, int beiDora)
        {
            Fu = fu;
            Yakus = yakuValues.ToArray();
            Fan = 0;
            IsQTJ = 青天井;
            IsSuperBingo = isSuperBingo;
            Dora = dora;
            UraDora = uraDora;
            RedDora = redDora;
            BeiDora = beiDora;
            Doras = Dora + UraDora + RedDora + BeiDora;
            IsYakuman = false;
            if (青天井)
            {
                foreach (var yaku in yakuValues)
                {
                    Fan += yaku.Type == YakuType.Yakuman ? yaku.Value * MahjongConstants.YakumanBaseFan : yaku.Value;
                    if (yaku.Type == YakuType.Yakuman) IsYakuman = true;
                }
            }
            else
            {
                foreach (var yaku in yakuValues)
                {
                    Fan += yaku.Value;
                    if (yaku.Type == YakuType.Yakuman) IsYakuman = true;
                }
            }
            FanWithoutDora = yakuValues.Sum(y => y.Type == YakuType.Yakuman ? y.Value * MahjongConstants.YakumanBaseFan : y.Value);

            if (yakuValues.Count == 0)
            {
                BasePoint = 0;
                TotalFan = 0;
                return;
            }

            if (青天井)
            {
                TotalFan = Fan + Doras;
                int point = Fu * (int)Math.Pow(2, TotalFan + 2);
                BasePoint = MahjongLogic.ToNextUnit(point, 100);
            }
            else if (IsYakuman)
            {
                BasePoint = MahjongConstants.Yakuman;
                TotalFan = MahjongConstants.YakumanBaseFan;
            }
            else
            {
                TotalFan = Fan + Doras;
                if (TotalFan >= 13) BasePoint = MahjongConstants.Yakuman;
                else if (TotalFan >= 11) BasePoint = MahjongConstants.Sanbaiman;
                else if (TotalFan >= 8) BasePoint = MahjongConstants.Baiman;
                else if (TotalFan >= 6) BasePoint = MahjongConstants.Haneman;
                else if (TotalFan >= 5) BasePoint = MahjongConstants.Mangan;
                else if (IsSuperBingo)
                {
                    if (TotalFan == 4) BasePoint = MahjongConstants.Mangan;
                    else if (TotalFan == 3) BasePoint = 1000;
                    else if (TotalFan == 2) BasePoint = 500;
                    else BasePoint = 300;
                }
                else
                {
                    int point = Fu * (int)Math.Pow(2, TotalFan + 2);
                    BasePoint = Math.Min(MahjongConstants.Mangan, point);
                }
            }
            Array.Sort(Yakus);
        }

        public PointInfo(NetworkPointInfo netInfo)
            : this(netInfo.Fu, netInfo.YakuValues, netInfo.IsQTJ, netInfo.IsSuperBingo, netInfo.Dora, netInfo.UraDora, netInfo.RedDora, netInfo.BeiDora)
        {
        }

        public int BasePoint { get; }
        public int TotalFan { get; }
        public int FanWithoutDora { get; }

        public IList<YakuValue> YakuList
        {
            get
            {
                return new List<YakuValue>(Yakus);
            }
        }

        public override string ToString()
        {
            var yakus = Yakus == null ? "" : string.Join(", ", Yakus.Select(yaku => yaku.ToString()));
            return
                $"Fu = {Fu}, Fan = {Fan}, Dora = {Dora}, UraDora = {UraDora}, RedDora = {RedDora}, BeiDora = {BeiDora}, "
                + $"Yakus = [{yakus}], BasePoint = {BasePoint}, isSuperBingo = {IsSuperBingo}";
        }

        public int CompareTo(PointInfo other)
        {
            var basePointComparison = BasePoint.CompareTo(other.BasePoint);
            if (basePointComparison != 0) return basePointComparison;
            var fanComparison = TotalFan.CompareTo(other.TotalFan);
            if (fanComparison != 0) return fanComparison;
            return Fu.CompareTo(other.Fu);
        }

        public int calculateTotalPoints(bool isDealer, bool isTsumo, int numPlayers)
        {
            int totalMultiplier = getTotalMultiplier(isDealer);
            if (IsSuperBingo && TotalFan < 5)
            {
                if (TotalFan == 4)
                {
                    return MahjongConstants.Mangan * totalMultiplier;
                }
                else if (TotalFan == 3)
                {
                    return 1000 * totalMultiplier;
                }
                else if (TotalFan == 2)
                {
                    return isDealer 
                        ? isTsumo ? 4000 : 3000 
                        : 2000;
                }
                else
                {
                    return isDealer || isTsumo ? 2000 : 1000;
                }
            }
            // Accounts for super bingo
            if (isTsumo)
            {
                int dealerPayment = isDealer ? calculateTsumoNonDealerPayment(isDealer) : calculateTsumoDealerPayment();
                int nonDealerPayment = calculateTsumoNonDealerPayment(isDealer);
                return isDealer ? nonDealerPayment * (numPlayers - 1) : dealerPayment + nonDealerPayment * (numPlayers - 2);
            }
            else return MahjongLogic.ToNextUnit(BasePoint * totalMultiplier, 100);
            
        }

        public int calculateTsumoDealerPayment()
        {
            if (IsSuperBingo)
            {
                if (TotalFan >= 13) return 20000;
                else if (TotalFan >= 11) return 16000;
                else if (TotalFan >= 8) return 10000;
                else if (TotalFan >= 6) return 8000;
                else if (TotalFan >= 4) return 5000;
                else if (TotalFan == 3) return 3000;
                else if (TotalFan == 2) return 1000;
                else return 1000;
            }
            return MahjongLogic.ToNextUnit(BasePoint * 2, 100);
        }

        public int calculateTsumoNonDealerPayment(bool isDealer)
        {
            if (IsSuperBingo)
            {
                if (TotalFan >= 13) return isDealer ? 24000 : 12000;
                else if (TotalFan >= 11) return isDealer ? 18000 : 8000;
                else if (TotalFan >= 8) return isDealer ? 12000 : 6000;
                else if (TotalFan >= 6) return isDealer ? 9000 : 4000;
                else if (TotalFan >= 4) return isDealer ? 6000 : 3000;
                else if (TotalFan == 3) return isDealer ? 3000 : 1000;
                else if (TotalFan == 2) return isDealer ? 2000 : 1000;
                else return 1000;
            }
            return isDealer ? calculateTsumoDealerPayment() : MahjongLogic.ToNextUnit(BasePoint, 100);
        }

        private int getTotalMultiplier(bool isDealer)
        {
            if (isDealer) return 6;
            else return 4;
        }
    }
}