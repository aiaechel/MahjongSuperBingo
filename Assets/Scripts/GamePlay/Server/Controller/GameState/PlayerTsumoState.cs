using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GamePlay.Client.Controller;
using GamePlay.Server.Model;
using GamePlay.Server.Model.Events;
using Mahjong.Model;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GamePlay.Server.Controller.GameState
{
    public class PlayerTsumoState : ServerState, IOnEventCallback
    {
        public int TsumoPlayerIndex;
        public Tile WinningTile;
        public MahjongSet MahjongSet;
        public PointInfo TsumoPointInfo;
        private IList<PointTransfer> transfers;
        private bool[] responds;
        private float serverTimeOut;
        private float firstTime;
        private const float ServerMaxTimeOut = 10;

        public override void OnServerStateEnter()
        {
            PhotonNetwork.AddCallbackTarget(this);
            bool isDealer = CurrentRoundStatus.IsDealer(TsumoPlayerIndex);
            int numPlayers = players.Count;
            int multiplier = gameSettings.GetMultiplier(isDealer, numPlayers);
            var netInfo = new NetworkPointInfo
            {
                Fu = TsumoPointInfo.Fu,
                YakuValues = TsumoPointInfo.YakuList.ToArray(),
                Dora = TsumoPointInfo.Dora,
                UraDora = TsumoPointInfo.UraDora,
                RedDora = TsumoPointInfo.RedDora,
                IsQTJ = TsumoPointInfo.IsQTJ,
                IsSuperBingo = TsumoPointInfo.IsSuperBingo
            };
            var info = new EventMessages.TsumoInfo
            {
                TsumoPlayerIndex = TsumoPlayerIndex,
                TsumoPlayerName = CurrentRoundStatus.GetPlayerName(TsumoPlayerIndex),
                TsumoHandData = CurrentRoundStatus.HandData(TsumoPlayerIndex),
                WinningTile = WinningTile,
                DoraIndicators = MahjongSet.DoraIndicators,
                UraDoraIndicators = MahjongSet.UraDoraIndicators,
                IsRichi = CurrentRoundStatus.RichiStatus(TsumoPlayerIndex),
                TsumoPointInfo = netInfo,
                TotalPoints = TsumoPointInfo.calculateTotalPoints(isDealer, true, numPlayers)
            };
            // send rpc calls
            ClientBehaviour.Instance.photonView.RPC("RpcTsumo", RpcTarget.AllBufferedViaServer, info);
            // get point transfers
            // todo -- tsumo loss related, now there is tsumo loss by default

            transfers = new List<PointTransfer>();
            for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
            {
                if (playerIndex == TsumoPlayerIndex) continue;
                int amount = CurrentRoundStatus.IsDealer(playerIndex) 
                    ? TsumoPointInfo.calculateTsumoDealerPayment() 
                    : TsumoPointInfo.calculateTsumoNonDealerPayment(isDealer);
                int extraPoints = CurrentRoundStatus.ExtraPoints;
                transfers.Add(new PointTransfer
                {
                    From = playerIndex,
                    To = TsumoPlayerIndex,
                    Amount = amount + extraPoints
                });
            }
            // richi-sticks-points
            transfers.Add(new PointTransfer
            {
                From = -1,
                To = TsumoPlayerIndex,
                Amount = CurrentRoundStatus.RichiSticksPoints
            });
            responds = new bool[players.Count];
            // determine server time out
            serverTimeOut = ServerMaxTimeOut + ServerConstants.ServerTimeBuffer;
            firstTime = Time.time;
        }

        public override void OnServerStateExit()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override void OnStateUpdate()
        {
            if (Time.time - firstTime > serverTimeOut || responds.All(r => r))
            {
                PointTransfer();
                return;
            }
        }

        private void PointTransfer()
        {
            var next = CurrentRoundStatus.OyaPlayerIndex != TsumoPlayerIndex;
            ServerBehaviour.Instance.PointTransfer(transfers, next, !next, false);
        }

        private void OnClientReadyEvent(int index)
        {
            responds[index] = true;
        }

        public void OnEvent(EventData photonEvent)
        {
            var code = photonEvent.Code;
            var info = photonEvent.CustomData;
            Debug.Log($"{GetType().Name} receives event code: {code} with content {info}");
            switch (code)
            {
                case EventMessages.ClientReadyEvent:
                    OnClientReadyEvent((int)photonEvent.CustomData);
                    break;
            }
        }
    }
}