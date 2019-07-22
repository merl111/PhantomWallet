﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using LunarLabs.Parser.JSON;
using Phantasma.Blockchain.Contracts.Native;
using Phantasma.Cryptography;
using Phantasma.Numerics;
using Phantasma.RpcClient.DTOs;
using Phantasma.Storage;
using Phantom.Wallet.DTOs;
using Newtonsoft.Json;

namespace Phantom.Wallet.Helpers
{
    public static class Utils
    {
      public static string GetTxAmount(TransactionDto tx, List<ChainDto> phantasmaChains, List<TokenDto> phantasmaTokens)
      {
          string amountsymbol = null;
          string typetx = null;

          string senderToken = null;
          Address senderChain = Address.FromText(tx.ChainAddress);
          Address senderAddress = Address.Null;

          string receiverToken = null;
          Address receiverChain = Address.Null;
          Address receiverAddress = Address.Null;

          BigInteger amount = 0;

          foreach (var evt in tx.Events) //todo move this
          {
              switch (evt.EventKind)
              {
                  case EventKind.TokenSend:
                      {
                          var data = Serialization.Unserialize<TokenEventData>(evt.Data.Decode());
                          amount = data.value;
                          senderAddress = Address.FromText(evt.EventAddress);
                          senderToken = data.symbol;
                          var amountDecimal = UnitConversion.ToDecimal(amount, phantasmaTokens.Single(p => p.Symbol == senderToken).Decimals);
                          amountsymbol = $"{amountDecimal} {senderToken}";
                      }
                      break;

                  case EventKind.TokenReceive:
                      {
                          var data = Serialization.Unserialize<TokenEventData>(evt.Data.Decode());
                          amount = data.value;
                          receiverAddress = Address.FromText(evt.EventAddress);
                          receiverChain = data.chainAddress;
                          receiverToken = data.symbol;
                          var amountDecimal = UnitConversion.ToDecimal(amount, phantasmaTokens.Single(p => p.Symbol == receiverToken).Decimals);
                          amountsymbol = $"{amountDecimal} {receiverToken}";
                      }
                      break;

                  case EventKind.TokenEscrow:
                      {
                          var data = Serialization.Unserialize<TokenEventData>(evt.Data.Decode());
                          amount = data.value;
                          receiverAddress = Address.FromText(evt.EventAddress);
                          receiverChain = data.chainAddress;
                          var amountDecimal = UnitConversion.ToDecimal(amount, phantasmaTokens.Single(p => p.Symbol == data.symbol).Decimals);
                          amountsymbol = $"{amountDecimal} {data.symbol}";
                      }
                      break;
              }
          }

          return amountsymbol;
      }

      public static string GetTxType(TransactionDto tx, List<ChainDto> phantasmaChains, List<TokenDto> phantasmaTokens)
      {
          string typetx = null;

          foreach (var evt in tx.Events) //todo move this
          {
              switch (evt.EventKind)
              {
                  case EventKind.TokenSend:
                      {
                          typetx = $"Transfer";
                      }
                      break;

                  case EventKind.TokenReceive:
                      {
                          typetx = $"Transfer";
                      }
                      break;
              }

              if (typetx == null)
              {
                typetx = $"Custom";
              }
          }

          return typetx;
      }

        public static string GetTxDescription(TransactionDto tx, List<ChainDto> phantasmaChains, List<TokenDto> phantasmaTokens)
        {
            string description = null;

            string senderToken = null;
            Address senderChain = Address.FromText(tx.ChainAddress);
            Address senderAddress = Address.Null;

            string receiverToken = null;
            Address receiverChain = Address.Null;
            Address receiverAddress = Address.Null;

            BigInteger amount = 0;

            foreach (var evt in tx.Events) //todo move this
            {
                switch (evt.EventKind)
                {
                    case EventKind.TokenSend:
                        {
                            var data = Serialization.Unserialize<TokenEventData>(evt.Data.Decode());
                            amount = data.value;
                            senderAddress = Address.FromText(evt.EventAddress);
                            senderToken = data.symbol;
                        }
                        break;

                    case EventKind.TokenReceive:
                        {
                            var data = Serialization.Unserialize<TokenEventData>(evt.Data.Decode());
                            amount = data.value;
                            receiverAddress = Address.FromText(evt.EventAddress);
                            receiverChain = data.chainAddress;
                            receiverToken = data.symbol;
                        }
                        break;

                    case EventKind.TokenEscrow:
                        {
                            var data = Serialization.Unserialize<TokenEventData>(evt.Data.Decode());
                            amount = data.value;
                            var amountDecimal = UnitConversion.ToDecimal(amount, phantasmaTokens.Single(p => p.Symbol == data.symbol).Decimals);
                            receiverAddress = Address.FromText(evt.EventAddress);
                            receiverChain = data.chainAddress;
                            var chain = GetChainName(receiverChain.Text, phantasmaChains);
                            description =
                                $"escrowed for address {receiverAddress} in {chain}";
                        }
                        break;
                    case EventKind.AddressRegister:
                        {
                            var name = Serialization.Unserialize<string>(evt.Data.Decode());
                            description = $"{evt.EventAddress} registered the name '{name}'";
                        }
                        break;

                    case EventKind.AddFriend:
                        {
                            var address = Serialization.Unserialize<Address>(evt.Data.Decode());
                            description = $"{evt.EventAddress} added '{address.ToString()} to friends.'";
                        }
                        break;

                    case EventKind.RemoveFriend:
                        {
                            var address = Serialization.Unserialize<Address>(evt.Data.Decode());
                            description = $"{evt.EventAddress} removed '{address.ToString()} from friends.'";
                        }
                        break;
                }
            }

            if (description == null)
            {
                if (amount > 0 && senderAddress != Address.Null && receiverAddress != Address.Null &&
                    senderToken != null && senderToken == receiverToken)
                {
                    var amountDecimal = UnitConversion.ToDecimal(amount, phantasmaTokens.Single(p => p.Symbol == senderToken).Decimals);

                    description =
                        $"sent from {senderAddress.ToString()} to {receiverAddress.ToString()}";
                }
                else if (amount > 0 && receiverAddress != Address.Null && receiverToken != null)
                {
                    var amountDecimal = UnitConversion.ToDecimal(amount, phantasmaTokens.Single(p => p.Symbol == receiverToken).Decimals);

                    description = $"received on {receiverAddress.Text} ";
                }
                else
                {
                    description = "Custom transaction";
                }

                if (receiverChain != Address.Null && receiverChain != senderChain)
                {
                    description +=
                        $" from {GetChainName(senderChain.Text, phantasmaChains)} chain to {GetChainName(receiverChain.Text, phantasmaChains)} chain";
                }
            }

            return description;
        }

        private static string GetChainName(string address, List<ChainDto> phantasmaChains)
        {
            foreach (var element in phantasmaChains)
            {
                if (element.Address == address) return element.Name;
            }

            return string.Empty;
        }

        public static T DeserializeConfig<T>(string path)
        {
            return (T) JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }

        public static void SerializeConfig<T>(T walletConfig, string path)
        {
            if (path == null || path == "")
            {
                Console.WriteLine("Path cannot be empty!");
                return;
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(walletConfig));
        }

        public static decimal GetCoinRate(uint ticker, string symbol = "USD")
        {
            var url = $"https://api.coinmarketcap.com/v2/ticker/{ticker}/?convert={symbol}";

            string json;

            try
            {
                using (var wc = new WebClient())
                {
                    json = wc.DownloadString(url);
                }

                var root = JSONReader.ReadFromString(json);

                root = root["data"];
                var quotes = root["quotes"][symbol];

                var price = quotes.GetDecimal("price");

                return price;
            }
            catch
            {
                return 0;
            }
        }
    }
}
