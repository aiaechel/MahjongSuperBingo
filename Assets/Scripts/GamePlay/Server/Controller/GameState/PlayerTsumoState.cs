using System.Collections.Generic;
using System.Linq;
using GamePlay.Server.Model;
using GamePlay.Server.Model.Messages;
using Mahjong.Logic;
using Mahjong.Model;
using UnityEngine;
using UnityEngine.Networking;

namespace GamePlay.Server.Controller.GameState
{
    public class PlayerTsumoState : ServerState
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
            NetworkServer.RegisterHandler(MessageIds.ClientReadinessMessage, OnReadinessMessageReceived);
            int multiplier = gameSettings.GetMultiplier(CurrentRoundStatus.IsDealer(TsumoPlayerIndex), players.Count);
            var netInfo = new NetworkPointInfo
            {
                Fu = TsumoPointInfo.Fu,
                YakuValues = TsumoPointInfo.YakuList.ToArray(),
                Dora = TsumoPointInfo.Dora,
                UraDora = TsumoPointInfo.UraDora,
                RedDora = TsumoPointInfo.RedDora,
                IsQTJ = TsumoPointInfo.IsQTJ
            };
            var tsumoMessage = new ServerPlayerTsumoMessage
            {
                TsumoPlayerIndex = TsumoPlayerIndex,
                TsumoPlayerName = players[TsumoPlayerIndex].PlayerName,
                TsumoHandData = CurrentRoundStatus.HandData(TsumoPlayerIndex),
                WinningTile = WinningTile,
                DoraIndicators = MahjongSet.DoraIndicators,
                UraDoraIndicators = MahjongSet.UraDoraIndicators,
                IsRichi = CurrentRoundStatus.RichiStatus(TsumoPlayerIndex),
                TsumoPointInfo = netInfo,
                TotalPoints = TsumoPointInfo.BasePoint * multiplier
            };
            for (int i = 0; i < players.Count; i++)
            {
                players[i].connectionToClient.Send(MessageIds.ServerTsumoMessage, tsumoMessage);
            }
            // get point transfers
            // todo -- tsumo loss related, now there is tsumo loss by default
            transfers = new List<PointTransfer>();
            for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
            {
                if (playerIndex == TsumoPlayerIndex) continue;
                int amount = TsumoPointInfo.BasePoint;
                if (CurrentRoundStatus.IsDealer(playerIndex)) amount *= 2;
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

        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientReadinessMessage>();
            Debug.Log($"[Server] Received ClientReadinessMessage: {content}");
            if (content.Content != MessageIds.ServerPointTransferMessage)
            {
                Debug.LogError("The message contains invalid content.");
                return;
            }
            responds[content.PlayerIndex] = true;
        }

        public override void OnServerStateExit()
        {
            NetworkServer.UnregisterHandler(MessageIds.ClientReadinessMessage);
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
    }
}