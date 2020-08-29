using Miyabi.Asset.Client;
using Miyabi.Asset.Models;
using Miyabi.ClientSdk;
using Miyabi.Common.Models;
using Miyabi.Cryptography;
using System;
using System.Threading.Tasks;
using Utility;




namespace TestSample
{
    class Program
    {
        const string TableName = "ChaChaCoin";

        static async Task Main(string[] args)
        {

            var config = new SdkConfig(Utils.ApiUrl);
            var client = new Client(config);
            var _generalClient = new GeneralApi(client);
            // Ver2 implements module system. To enable modules, register is required.
            AssetTypesRegisterer.RegisterTypes();

            string txID_1 = await CreateAssetTable(client);
            string txID_2 = await AssetGenerate(client);
            string txID_3 = await Send(client);
            await ShowAsset(client);
            Console.WriteLine($"txid={txID_1}");
            Console.WriteLine($"txid={txID_2}");
            Console.WriteLine($"txid={txID_3}");

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static async Task<string> CreateAssetTable(IClient client)
        {
            

            var aliceAddress = new PublicKeyAddress(Utils.GetOwnerKeyPair().PublicKey);
            var assetTable = new CreateTable(new AssetTableDescriptor(
               TableName, false, false, new[] { aliceAddress }));

            var memo = new MemoEntry(new[] { "Hinatazaka_Token" });
            var tx = TransactionCreator.SimpleSignedTransaction(
                new ITransactionEntry[] { assetTable, memo },
                new[] { Utils.GetTableAdminKeyPair().PrivateKey });

            await SendTransaction(tx);

            return tx.Id.ToString();
        }

        public static async Task<string> Assetgenerate(IClient client)
        {


            var generateAsset = new AssetGen(TableName, 1000000m,
                new PublicKeyAddress(Utils.GetUser0KeyPair().PublicKey));

            var tx = TransactionCreator.SimpleSignedTransaction(
                new ITransactionEntry[] { generateAsset },
                new[] { Utils.GetOwnerKeyPair().PrivateKey });


            await SendTransaction(tx);

            return tx.Id.ToString(); ;
        }

        public static async Task<string> Send(IClient client)
        {
            var from = new PublicKeyAddress(Utils.GetUser0KeyPair());
            var to = new PublicKeyAddress(Utils.GetUser1KeyPair());

            var amount = Inputjudement();

            var moveCoin = new AssetMove(TableName, amount, from, to);
            var tx = TransactionCreator.SimpleSignedTransaction(
                new ITransactionEntry[] { moveCoin },
                new [] {Utils.GetUser0KeyPair().PrivateKey});
            await SendTransaction(tx);

            return tx.Id.ToString();
        }

        public static decimal Inputjudement()
        {
            const int RETRY_MAX = 5;
            decimal value = 0m;
            string Inputbycustomer;
            int i = 0;


            for (i = 0; i < RETRY_MAX; i++)
            {
                Console.WriteLine("Please enter tne send amount");
                Inputbycustomer = null;
                Inputbycustomer = Console.ReadLine();
                try
                {
                    value = Convert.ToDecimal(Inputbycustomer);
                    break;
                }
                catch (System.FormatException)
                {
                    Console.WriteLine("Not Number Please Retry");
                }
                catch (System.OverflowException)
                {
                    Console.Write("Overflow Exception");
                    Console.WriteLine("Please Retry input Number");
                }
            }

            if (i < RETRY_MAX)
            {
                return value;
            }
            else
            {
                Console.WriteLine("Types miss at fifth ! Finished Program");
                Environment.Exit(0x8020);
                return 0;
            }
        }

        private static async Task ShowAsset(IClient client)
        {
            // AssetClient has access to asset endpoints
            var assetClient = new AssetClient(client);

            var addresses = new Address[] {
                new PublicKeyAddress(Utils.GetUser0KeyPair()),
                new PublicKeyAddress(Utils.GetUser1KeyPair()),
            };

            foreach (var address in addresses)
            {
                var result = await assetClient.GetAssetAsync(TableName, address);
                Console.WriteLine($"address={address}, amount={result.Value}");
            }
        }

        public static async Task SendTransaction(Transaction tx)
        {
            var config = new SdkConfig(Utils.ApiUrl);
            var client = new Client(config);
            var _generalClient = new GeneralApi(client);


            try
            {
                var send = await _generalClient.SendTransactionAsync(tx);
                var result_code = send.Value;

                if (result_code != TransactionResultCode.Success
                   && result_code != TransactionResultCode.Pending)
                {
                    Console.WriteLine("取引が拒否されました!:\t{0}", result_code);

                }
            }
            catch (Exception e)
            {
                Console.Write("例外を検知しました！{e}");
            }

        }
    }
}