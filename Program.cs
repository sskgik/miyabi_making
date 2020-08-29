using Miyabi.Asset.Client;
using Miyabi.Asset.Models;
using Miyabi.ClientSdk;
using Miyabi.Common.Models;
using System;
using System.Threading.Tasks;
using Utility;

namespace AssetSample
{
    class Program
    {
        const string TableName = "ChaChaCOIN";
        
        static async Task Main(string[] args)
        {

            var config = new SdkConfig(Utils.ApiUrl);
            var client = new Client(config);
            var _generalClient = new GeneralApi(client);
            // Ver2 implements module system. To enable modules, register is required.
            AssetTypesRegisterer.RegisterTypes();

            string txID_1 = await CreateAssetTable(client);
            //string txID_2 = await AssetGenerate(client);
            Console.WriteLine($"txid={txID_1}");
            //Console.WriteLine($"txid={txID_2}");

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static async Task<string> CreateAssetTable(IClient client)
        {
            //テーブル名とテーブル所有者の公開鍵（トークン発行権や制限付き操作の実行権: 
            //PublicKey.Parse(ALICE_PUBLIC_KEY_ADDRESS)= Utils.GetOwnerKeyPair().PublicKey

            var aliceAddress = new PublicKeyAddress(Utils.GetOwnerKeyPair().PublicKey);
            var assetTable = new CreateTable(new AssetTableDescriptor(
               "ChaChaCoin", false, false, new[] { aliceAddress }));

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
                new PublicKeyAddress(Utils.GetOwnerKeyPair().PublicKey));

            var tx = TransactionCreator.SimpleSignedTransaction(
                new ITransactionEntry[] { generateAsset },
                new[] { Utils.GetOwnerKeyPair().PrivateKey });

            
            await SendTransaction(tx);

            return tx.Id.ToString(); ;
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
