using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using Phantasma.Cryptography;
using Phantasma.Numerics;
using Phantasma.VM;
using Phantasma.Storage;
using Phantom.Wallet.Controllers;
using Phantom.Wallet.Helpers;

using Newtonsoft.Json;


namespace PhantomCli
{
    class PhantomCli
    {
        private static KeyPair keyPair {get; set;} = null;

        private static AccountController AccountController { get; set; }

        private static CliWalletConfig config = new CliWalletConfig();

        private static String prompt { get; set; }

        public static void SetupControllers()
        {
            AccountController = new AccountController();
        }

        static void Main(string[] args)
        {
            string version = "0.1-alpha";
            config = Utils.ReadConfig<CliWalletConfig>(".config");
            prompt = config.Prompt;
            var startupMsg = "PhantomCli " + version;

            SetupControllers();
            List<string> completionList = new List<string>(lCommands.Keys); 
            Prompt.Run(
                ((command, listCmd, lists) =>
                {

                    string command_main = command.Trim().Split(new char[] { ' ' }).First();
                    string[] arguments = command.Split(new char[] { ' ' }).Skip(1).ToArray();
                    if (lCommands.ContainsKey(command_main))
                    {
                        Tuple<Action<string[]>, string> cmd = null;
                        lCommands.TryGetValue(command_main, out cmd);
                        Action<string[]> function_to_execute = cmd.Item1;
                        function_to_execute(arguments);
                    }
                    else
                        Console.WriteLine("Command '" + command_main + "' not found");
                    return null;
                }), prompt, startupMsg, completionList);
        }

        private static Dictionary<string, Tuple<Action<string[]>, string>> lCommands = 
            new Dictionary<string, Tuple<Action<string[]>, string>>()
        {
            { "help",       new Tuple<Action<string[]>, string>(HelpFunc,       "test")},
            { "exit",       new Tuple<Action<string[]>, string>(Exit,           "test")},
            { "clear",      new Tuple<Action<string[]>, string>(Clear,          "test")},
            { "wallet",     new Tuple<Action<string[]>, string>(CopyFunc,       "test")},
            { "tx",         new Tuple<Action<string[]>, string>(Transaction,    "test")},
            { "contract",   new Tuple<Action<string[]>, string>(ContractFunc,   "test")},
            { "invoke",     new Tuple<Action<string[]>, string>(InvokeFunc,     "test")},
            { "invokeTx",   new Tuple<Action<string[]>, string>(InvokeTxFunc,   "test")},
            { "history",    new Tuple<Action<string[]>, string>(HistoryFunc,    "test")},
            { "send",       new Tuple<Action<string[]>, string>(SendFunc,       "test")},
            { "test",       new Tuple<Action<string[]>, string>(TestFunc,       "test")},
            { "config",     new Tuple<Action<string[]>, string>(ConfigFunc,     "test")}
        };

        private static void ConfigFunc(string[] obj)
        {

            if (obj.Length < 1)
            {
                Console.WriteLine("Too less arguments");
                return;
            }

            string action = obj[0];
            string cfgItem = null; 
            string cfgValue = null;
            try
            {
                cfgItem = obj[1];
            }
            catch (IndexOutOfRangeException)
            {
                if (action == "set") 
                {
                    Console.WriteLine("Item cannot be empty!");
                    return;
                }
            }

            try
            {
                cfgValue = obj[2];
            }
            catch (IndexOutOfRangeException)
            {
                if (action == "set") 
                {
                    Console.WriteLine("Value cannot be empty!");
                    return;
                }
            }

            if (action == "set") {
                if (cfgItem == "prompt") 
                {
                    config.Prompt = cfgValue + " ";
                    prompt = cfgValue + " ";
                }
                else if (cfgItem == "currency")
                {
                    // TODO check if currency is available
                    config.Currency = cfgValue;
                }
                else if (cfgItem == "network")
                {
                    config.Network = cfgValue;
                }
                else
                {
                    Console.WriteLine("Config item not found!");
                }
                // write new cfg
                Utils.WriteConfig<CliWalletConfig>(config, ".config");

            }
            else if (action == "show") 
            {
                if (cfgItem == "prompt") 
                {
                    Console.WriteLine("Value: " + config.Prompt);
                }
                else if (cfgItem == "currency")
                {
                    Console.WriteLine("Value: " + config.Currency);
                }
                else if (cfgItem == "network")
                {
                    Console.WriteLine("Value: " + config.Network);
                }
                else
                {
                    Console.WriteLine(config.ToJson());
 
                }

            }
        }


        private static void TestFunc(string[] obj)
        {
            // only used for quick testing!
            //var json = @"{ 'parameters': [ { 'name': 'address', 'vmtype': 'Object', 'type': 'Phantasma.Cryptography.Address', 'input': 'P5ySorAXMaJLwe6AqTsshW3XD8ahkwNpWHU9KLX9CwkYd', 'info': 'info1' } ] }";
            //List<object> lst = SendUtils.BuildParamList(json);
            //lst.ForEach(Console.WriteLine);
            var json2 = @"{ 'parameters': [  { 'name': 'address', 'vmtype': 'Object', 'type': 'Phantasma.Cryptography.Address', 'input': 'P5ySorAXMaJLwe6AqTsshW3XD8ahkwNpWHU9KLX9CwkYd', 'info': 'info1' }, 
                                             { 'name': 'address', 'vmtype': 'Enum', 'type': 'Phantasma.Blockchain.ArchiveFlags', 'input': '1', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'Enum', 'type': 'Phantasma.Blockchain.ArchiveFlags', 'input': '1', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'Enum', 'type': 'Phantasma.Blockchain.Contracts.Native.ExchangeOrderSide', 'input': '1', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'Enum', 'type': 'Phantasma.Blockchain.Contracts.Native.ExchangeOrderSide', 'input': '2', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'Enum', 'type': 'Phantasma.Blockchain.Contracts.Native.ExchangeOrderType', 'input': '3', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'Enum', 'type': 'Phantasma.Blockchain.Contracts.Native.ExchangeOrderType', 'input': '4', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'Timestamp', 'type': 'Phantasma.Core.Types.Timestamp', 'input': '07/20/2019 20:04:30', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'Bool', 'type': 'System.Boolean', 'input': 'False', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'Bool', 'type': 'System.Boolean', 'input': 'True', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'String', 'type': 'System.String', 'input': 'ThisIsAString', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'Bytes', 'type': 'System.Byte[]', 'input': 'ThisIsAByteArray', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'Number', 'type': 'System.Int32', 'input': '987654321', 'info': 'info1' },
                                             { 'name': 'address', 'vmtype': 'Number', 'type': 'Phantasma.Numerics.BigInteger', 'input': '12345678', 'info': 'info1' } 
                                          ] }";
            List<object> lst2 = SendUtils.BuildParamList(json2);
            lst2.ForEach(Console.WriteLine);

        }

        private static void Transaction(string[] obj)
        {
            if (obj.Length > 1)
            {
                Console.WriteLine("Too many arguments");
                return;
            }

            if (obj.Length < 1)
            {
                Console.WriteLine("Too less arguments");
                return;
            }

            string txHash = obj[0];
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(AccountController
                    .GetTxConfirmations(txHash).Result, Formatting.Indented);
            Console.WriteLine(json);
        }

        private static void SendFunc(string[] obj)
        {
            string txHash = string.Join("", obj);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(AccountController
                    .GetTxConfirmations(txHash).Result, Formatting.Indented);
            Console.WriteLine(json);
        }

        private static void ContractFunc(string[] obj)
        {
            if (obj.Length > 2)
            {
                Console.WriteLine("Too many arguments");
                return;
            }

            if (obj.Length < 2)
            {
                Console.WriteLine("Too less arguments");
                return;
            }

            string chain = obj[0];
            string contract = obj[1];
            Console.WriteLine("Send");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(AccountController
                    .GetContractABI(chain, contract).Result, Formatting.Indented);
            Console.WriteLine("done");
            Console.WriteLine(json);
        }

        private static void HistoryFunc(string[] obj)
        {
            Console.WriteLine();
            foreach (List<char> item in Prompt.GetHistory()) 
            {
                Console.WriteLine(new string(item.ToArray()));
            }
            Console.WriteLine();
        }

        private static void InvokeTxFunc(string[] obj)
        {
            if (obj.Length > 3)
            {
                Console.WriteLine("Too many arguments");
                return;
            }

            if (obj.Length < 3)
            {
                Console.WriteLine("Too less arguments");
                return;
            }

            string chain = obj[0];
            string contract = obj[1];
            string method = obj[2];
            KeyPair kp = GetLoginKey();
            object[] paramArray = new object[] {kp.Address, kp.Address};
            var result = AccountController.InvokeContractTxGeneric(kp, chain, contract, method, paramArray).Result;
            if (result == null) {
                Console.WriteLine("Node returned null...");
                return;
            }

            Console.WriteLine("Result: " + result);

        }

        private static void InvokeFunc(string[] obj)
        {
            string chain = obj[0];
            string contract = obj[1];
            string method = obj[2];
            KeyPair kp = GetLoginKey();
            object[] paramArray = new object[] {kp.Address};
            var result = AccountController.InvokeContractGeneric(kp, chain, contract, method, paramArray).Result;
            if (result == null) {
                Console.WriteLine("Node returned null...");
                return;
            }

            Console.WriteLine("Result: " + result);

        }

        private static void Clear(string[] obj)
        {
            Console.Clear();
        }

        private static void Exit(string[] obj)
        {
            Environment.Exit(0);
        }

        private static void CopyFunc(string[] obj)
        {
            GetLoginKey();
        }

        private static KeyPair GetLoginKey(bool changeWallet=false)
        {
            if (keyPair == null && !changeWallet) 
            {
                Console.Write("Enter private key: ");
                var wif = Console.ReadLine();
                var kPair = KeyPair.FromWIF(wif);
                keyPair = kPair;
            }
            return keyPair;
        }


        public static void HelpFunc(string[] args)
        {
            Console.WriteLine("===== SOME MEANINGFULL HELP ==== ");
        }
    }
}
