using System;
using System.Runtime.Remoting;
using Golem.Game.Mobiles;
using Golem.Server.Database;
using Newtonsoft.Json;

namespace Golem.Game.Items
{
    /* Coins are divided into 4 levels: Copper, Silver, Gold, Platinum
     * 1 Platinum = 100 Gold
     * 1 Gold = 100 Silver
     * 1 Silver = 100 Copper
     * 
     * Expression as a fraction of 1 gold:
     * 1 Copper = 0.0001 ( * 100 == 0.1)
     * 1 Silver = 0.01 ( * 100 == 1.0)
     * 1 Gold = 1.0 ( * 100 = 100.0)
     * 1 Platinum = 100.0
     */

    
    public class CoinStack : IStorable
    {
        private const int OnePlatinum = (OneGold * 100);
        private const int OneGold = (OneSilver * 100);
        private const int OneSilver = (OneCopper * 100);
        private const int OneCopper = 1;

        /// <inheritdoc />
        public string Key { get; } = Guid.NewGuid().ToString();

        private Int64 _rawValue;
        public Int64 RawValue { get => _rawValue; set => SetRawValue(value); }

        [JsonIgnore]
        public int Copper { get; set; }
        [JsonIgnore]
        public int Silver { get; set; }
        [JsonIgnore]
        public int Gold { get; set; }
        [JsonIgnore]
        public int Platinum { get; set; }

        public CoinStack(long value)
        {
            SetRawValue(value);
        }

        public void AddCopper(int amount) => AddValue(amount * OneCopper);
        public void AddSilver(int amount) => AddValue(amount * OneSilver);
        public void AddGold(int amount) => AddValue(amount * OneGold);
        public void AddPlatinum(int amount) => AddValue(amount * OnePlatinum);

        private void SetRawValue(long rawValue)
        {
            _rawValue = 0;
            AddValue(rawValue);
        }

        /// <summary>
        /// Add coinage to this stack, expressed in copper peices.
        /// </summary>
        /// <param name="rawValue">Value to add, in copper</param>
        public void AddValue(long rawValue)
        {
            // Maximum coinage is 2,077,252,342 platinum, 77 gold, 58 silver and 7 copper.
            // Wouldn't expect anyone to hold more than 2billion platinum.

            long number = RawValue / OnePlatinum;
            Platinum = (int)number;
            RawValue -= number * OnePlatinum;

            number = RawValue / OneGold;
            Gold = (int)number;
            RawValue -= number * OneGold;

            number = RawValue / OneSilver;
            Silver = (int)number;
            RawValue -= number * OneSilver;

            Copper = (int)RawValue;
        }

        /// <summary>
        /// Create a CoinStack from a textual description
        /// </summary>
        /// <example>
        /// 1 platinum 3 copper
        /// 5 gold 3 copper
        /// 8 copper 2 gold
        /// 2 g 3 c
        /// </example>
        /// <param name="value"></param>
        /// <returns></returns>
        public static CoinStack FromString(string value)
        {
            int copper, silver, gold, platinum;
            copper = silver = gold = platinum = 0;

            var str = value.Split(' ');

            int currentNumber = 0;

            for (int i = 0; i < str.Length; i++)
            {
                if ("copper".StartsWith(str[i].ToLower()))
                    copper = currentNumber;
                
                else if ("silver".StartsWith(str[i].ToLower()))
                    silver = currentNumber;

                else if ("gold".StartsWith(str[i].ToLower()))
                    gold = currentNumber;

                else if ("platinum".StartsWith(str[i].ToLower()))
                    platinum = currentNumber;

                else
                    Int32.TryParse(str[i], out currentNumber);
            }

            return copper > 0 || silver > 0 || gold > 0 || platinum > 0
                ? new CoinStack((copper * OneCopper) + (silver * OneSilver) + (gold * OneGold) + (platinum * OnePlatinum))
                : null;
        }
    }
}