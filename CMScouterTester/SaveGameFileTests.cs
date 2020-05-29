using CMScouter.UI;
using CMScouterFunctions.DataClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMScouterTester
{
    [TestClass]
    public class SaveGameFileTests
    {
        protected const string LiverpoolSave = @"C:\Install\Games\CM 0102\A Large Oldham EndOfSeason.sav";
        protected const string OldhamSave = @"C:\Install\Games\CM 0102\Oldham Cheating.sav";
        static CMScouterUI cmsUI;

        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            cmsUI = new CMScouterUI(LiverpoolSave);
        }

        [TestMethod]
        [DataRow("VAN DER")]
        public void TestSearchByPlayerSecondName(string playerName)
        {
            List<PlayerView> players = cmsUI.GetPlayersBySecondName(playerName);

            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count > 0);
            Assert.IsTrue(players.All(p => p.SecondName.StartsWith(playerName, StringComparison.InvariantCultureIgnoreCase)));
        }

        //[DataRow(1233)] 
        //[DataRow(6898)] //Brown
        //[DataRow(5387)] //Lovren
        [DataRow(41005)] // Duarte
        //[DataRow(32377)] // Digne
        //[DataRow(29838)] // Sefil
        //[DataRow(2163)]
        //[DataRow(62797)]
        [TestMethod]
        public void TestSearchByPlayerId(int playerId1)
        {
            List<PlayerView> players = cmsUI.GetPlayerByPlayerId(new List<int>() { playerId1 });

            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count == 1);
            Assert.IsTrue(players.Any(p => p.PlayerId == playerId1));
        }

        [DataRow("LIVERPOOL")]
        [TestMethod]
        public void TestSearchByClubName(string clubName)
        {
            ScoutingRequest request = new ScoutingRequest() { ClubName = clubName };
            List<PlayerView> players = cmsUI.GetScoutResults(request);

            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count > 0);
            Assert.IsTrue(players.All(x => !string.IsNullOrEmpty(x.FirstName) && !string.IsNullOrEmpty(x.SecondName)));
            Assert.IsTrue(players.All(x => x.PlayerId > 0));
            Assert.IsTrue(players.All(x => !string.IsNullOrEmpty(x.ClubName)));
        }

        [TestMethod]
        [DataRow("HENRY")]
        public void TestSearchForUnemployedPlayer(string playerName)
        {
            List<PlayerView> players = cmsUI.GetPlayersBySecondName(playerName);

            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count > 0);
            Assert.IsTrue(players.All(p => p.SecondName.StartsWith(playerName, StringComparison.InvariantCultureIgnoreCase)));
            Assert.IsTrue(players.Any(p => p.ClubName == string.Empty));
        }

        [TestMethod]
        [DataRow("SCOTLAND")]
        public void TestSearchByNationality(string nationality)
        {
            int? nationID = cmsUI.GetAllNations().FirstOrDefault(x => x.Name.Equals(nationality, StringComparison.InvariantCultureIgnoreCase))?.Id;
            Assert.IsNotNull(nationID.Value);
            
            ScoutingRequest request = new ScoutingRequest() { Nationality = nationID.Value };
            List<PlayerView> players = cmsUI.GetScoutResults(request);

            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count > 0);
            Assert.IsTrue(players.All(p => p.Nationality.StartsWith(nationality, StringComparison.InvariantCultureIgnoreCase) || p.SecondaryNationality?.StartsWith(nationality, StringComparison.InvariantCultureIgnoreCase) == true));
        }
        
        [TestMethod]
        [DataRow()]
        public void TestSearchForHighestGKs()
        {
            ScoutingRequest request = new ScoutingRequest() { PlayerType = PlayerType.GoalKeeper, NumberOfResults = 20 };
            List<PlayerView> players = cmsUI.GetScoutResults(request);
            
            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count > 0);
            Assert.IsTrue(players.All(p => p.ScoutRatings.Goalkeeper.BestRole().Rating > 70));
            Assert.IsTrue(players.All(p => p.CurrentAbility > 80));
        }

        [TestMethod]
        [DataRow()]
        public void TestSearchForBestFreeTransfers()
        {
            ScoutingRequest request = new ScoutingRequest() { MaxValue = 0, NumberOfResults = 100 };
            List<PlayerView> players = cmsUI.GetScoutResults(request);

            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count >= 20);
        }

        [TestMethod]
        [DataRow()]
        public void TestSearchForCentreBacks()
        {
            ScoutingRequest request = new ScoutingRequest() { PlayerType = PlayerType.CentreHalf, MaxValue = 2000000, NumberOfResults = 50, EUNationalityOnly = true };
            List<PlayerView> players = cmsUI.GetScoutResults(request);

            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count >= 20);
            Assert.IsTrue(players.All(p => p.ScoutRatings.CentreHalf.BestRole().Rating > 50));
        }

        [TestMethod]
        [DataRow()]
        public void TestSearchForAttackingMidfielders()
        {
            ScoutingRequest request = new ScoutingRequest() { PlayerType = PlayerType.AttackingMidfielder, MaxValue = 2000000, NumberOfResults = 50, EUNationalityOnly = true };
            List<PlayerView> players = cmsUI.GetScoutResults(request);

            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count >= 20);
            Assert.IsTrue(players.All(p => p.ScoutRatings.AttackingMidfielder.BestRole().Rating > 50));
        }

        [TestMethod]
        [DataRow("RAMSES BECKER")]
        public void TestGoalkeeperRatings(string playerSurname)
        {
            List<PlayerView> players = cmsUI.GetPlayersBySecondName(playerSurname);
            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count >= 1);

            var keeper = players.OrderByDescending(p => p.ScoutRatings.Goalkeeper.BestRole().Rating).First();

            Assert.IsNotNull(keeper);
            Assert.IsTrue(keeper.ScoutRatings.Goalkeeper.BestRole().Rating > 70);
        }

        [TestMethod]
        [DataRow("OLDHAM", "BROWN")]
        [DataRow("LIVERPOOL", "VAN DIJK")]
        public void TestCentreHalfRatings(string clubName, string playerSurname)
        {
            ScoutingRequest request = new ScoutingRequest() { ClubName = clubName };
            List<PlayerView> players = cmsUI.GetScoutResults(request);
            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count > 0);

            var player = players.First(f => f.SecondName.Equals(playerSurname, StringComparison.InvariantCultureIgnoreCase));

            Assert.IsNotNull(player);
            //Assert.IsTrue(player.ScoutRatings.CentreHalf.BestRole().Rating > 50 && player.ScoutRatings.CentreHalf.BestRole().Rating < 60);
        }

        [TestMethod]
        [DataRow("LIVERPOOL", "FIRMINO")]
        public void TestAttackingMidfielderRatings(string clubName, string playerSurname)
        {
            ScoutingRequest request = new ScoutingRequest() { ClubName = clubName };
            List<PlayerView> players = cmsUI.GetScoutResults(request);
            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count > 0);

            var player = players.First(f => f.SecondName.Equals(playerSurname, StringComparison.InvariantCultureIgnoreCase));

            Assert.IsNotNull(player);
            Assert.IsTrue(player.ScoutRatings.AttackingMidfielder.BestRole().Rating > 70);
        }

        [TestMethod]
        [DataRow("LIVERPOOL", "SALAH")]
        public void TestStrikerRatings(string clubName, string playerSurname)
        {
            ScoutingRequest request = new ScoutingRequest() { ClubName = clubName };
            List<PlayerView> players = cmsUI.GetScoutResults(request);
            Assert.IsNotNull(players);
            Assert.IsTrue(players.Count > 0);

            var player = players.First(f => f.SecondName.Equals(playerSurname, StringComparison.InvariantCultureIgnoreCase));

            Assert.IsNotNull(player);
            Assert.IsTrue(player.ScoutRatings.CentreForward.BestRole().Rating > 70);
        }
    }
}
