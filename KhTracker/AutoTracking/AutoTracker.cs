﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections;
//using System.IO;

namespace KhTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MemoryReader memory;

        private Int32 ADDRESS_OFFSET;
        private static DispatcherTimer aTimer;//, autoTimer;
        private List<ImportantCheck> importantChecks;
        private Ability highJump;
        private Ability quickRun;
        private Ability dodgeRoll;
        private Ability aerialDodge;
        private Ability glide;

        private Ability secondChance;
        private Ability onceMore;

        private DriveForm valor;
        private DriveForm wisdom;
        private DriveForm master;
        private DriveForm limit;
        private DriveForm final;
        private DriveForm anti;

        private Magic fire;
        private Magic blizzard;
        private Magic thunder;
        private Magic magnet;
        private Magic reflect;
        private Magic cure;

        private Report reportItem;
        private Summon charmItem;
        private ImportantCheck proofItem;
        private ImportantCheck visitItem;
        private ImportantCheck extraItem;

        private TornPage pages;

        private World world;
        private Stats stats;
        private Rewards rewards;
        private List<ImportantCheck> collectedChecks;
        private List<ImportantCheck> newChecks;
        private List<ImportantCheck> previousChecks;

        private int fireLevel;
        private int blizzardLevel;
        private int thunderLevel;
        private int cureLevel;
        private int reflectLevel;
        private int magnetLevel;
        private int tornPageCount;

        private CheckEveryCheck checkEveryCheck;

        public static bool pcsx2tracking = false; //game version
        private bool onContinue = false; //for death counter
        private bool eventInProgress = false; //boss detection

        private int[] temp = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private int[] tempPre = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        ///Auto-Detect Control Stuff
        //private bool autoDetected = false; 
        //public int storedDetectedVersion = 0; // 0 = nothing detected, 1 = PC, 2 = PCSX2
        //private bool isWorking = false;
        //private bool firstRun = true;       

        ///
        /// Autotracking Startup
        ///

        public void InitPCSX2Tracker(object sender, RoutedEventArgs e)
        {
            pcsx2tracking = true;
            InitAutoTracker(true);
        }

        public void InitPCTracker(object sender, RoutedEventArgs e)
        {
            pcsx2tracking = false;
            InitAutoTracker(false);
        }

        //public void SetAutoDetectTimer()
        //{
        //    //return if autotracking already succsessful
        //    if (isWorking || !AutoDetectOption.IsChecked)
        //        return;
        //
        //    //if autotracking isn't currently working then stop timer
        //    if (aTimer != null)
        //        aTimer.Stop();
        //
        //    if (firstRun)
        //    {
        //        autoTimer = new DispatcherTimer();
        //        autoTimer.Tick += InitAutoDetect;
        //        firstRun = false;
        //        autoTimer.Interval = new TimeSpan(0, 0, 0, 5, 0); // attempt tracking every 5 seconds
        //    }
        //
        //    autoTimer.Start();
        //}

        //private void InitAutoDetect(object sender, EventArgs e)
        //{
        //    int hooktries = 0;
        //    bool version = true; //Reminder: true = emu | false = pc
        //
        //    //if auto-detect was sucessful before then attempt re-autotracking based on that
        //    if (autoDetected && storedDetectedVersion != 0)
        //    {
        //        do
        //        {
        //            memory = new MemoryReader(pcsx2tracking);
        //            if (hooktries < 20)
        //            {
        //                hooktries++;
        //            }
        //            else
        //            {
        //                memory = null;
        //                return;
        //            }
        //        } while (!memory.Hooked);
        //
        //        //stop auto-detect timer
        //        if (autoTimer != null)
        //            autoTimer.Stop();
        //
        //        firstRun = true;                  //reset firstrun
        //        isWorking = true;
        //        InitAutoTracker(pcsx2tracking);   //start rest of autotracking
        //        return;
        //    }
        //
        //    //attempt tracking correct version
        //    do
        //    {
        //        memory = new MemoryReader(version);
        //
        //        //try emu hooking 1st
        //        if (hooktries <= 10 && version)
        //        {
        //            hooktries++;
        //        }
        //        else
        //        {
        //            //could not hook emu so change version to try pc hooking
        //            version = false;
        //        }
        //
        //        //try pc hooking if emu failed
        //        if (hooktries <= 20 && !version)
        //        {
        //            hooktries++;
        //        }
        //        else if (hooktries > 20)
        //        {
        //            //could not hook pc so reset and return to try again next tick
        //            memory = null;
        //            autoDetected = false;
        //            storedDetectedVersion = 0;
        //            return;
        //        }
        //
        //    } while (!memory.Hooked);
        //
        //    if (memory.Hooked)
        //    {
        //        //stop auto-detect timer
        //        if (autoTimer != null)
        //            autoTimer.Stop();
        //
        //        //store version for disconnect
        //        if (version)
        //            storedDetectedVersion = 2;
        //        else
        //            storedDetectedVersion = 1;
        //
        //        autoDetected = true;        //autodetect success
        //        isWorking = true;
        //        firstRun = true;            //reset frstrun
        //        pcsx2tracking = version;    //set which version we are tracking
        //        InitAutoTracker(version);   //start rest of autotracking
        //    }
        //}

        public void InitAutoTracker(bool PCSX2)
        {
            int tries = 0;
            //try at least 20 times before giving error.
            do
            {
                memory = new MemoryReader(PCSX2);
                if (tries < 20)
                {
                    tries++;
                }
                else
                {
                    memory = null;
                    MessageBox.Show("Please start KH2 before loading the Auto Tracker.");
                    return;
                }
            } while (!memory.Hooked); // && !autoDetected);

            // PC Address anchors
            int Now = 0x0714DB8;
            int Save = 0x09A70B0;
            int Sys3 = 0x2A59DF0;
            int Bt10 = 0x2A74880;
            int BtlEnd = 0x2A0D3E0;
            int Slot1 = 0x2A20C98;

            if (PCSX2 == false)
            {
                //change connection icon visual and start pc version setup
                //NOTE: removed title check for now. i'm unsure if it was actually ever needed.
                Connect.Visibility = Visibility.Collapsed;
                Connect2.Source = data.AD_PCred;
                Connect2.Visibility = Visibility.Visible;
                FinishSetupPC(PCSX2, Now, Save, Sys3, Bt10, BtlEnd, Slot1);
            }
            else
            {
                try
                {
                    findAddressOffset();
                }
                catch (Win32Exception)
                {
                    memory = null;
                    MessageBox.Show("Unable to access PCSX2 try running KHTracker as admin");
                    //isWorking = false;
                    //SetAutoDetectTimer();
                    return;
                }
                catch
                {
                    memory = null;
                    MessageBox.Show("Error connecting to PCSX2");
                    //isWorking = false;
                    //SetAutoDetectTimer();
                    return;
                }

                // PCSX2 anchors 
                Now = 0x032BAE0;
                Save = 0x032BB30;
                Sys3 = 0x1CCB300;
                Bt10 = 0x1CE5D80;
                BtlEnd = 0x1D490C0;
                Slot1 = 0x1C6C750;

                //change connection icon visual and start final setup
                Connect.Visibility = Visibility.Collapsed;
                Connect2.Source = data.AD_PS2;
                Connect2.Visibility = Visibility.Visible;
                FinishSetup(PCSX2, Now, Save, Sys3, Bt10, BtlEnd, Slot1);
            }
        }

        private void findAddressOffset()
        {
            bool found = false;
            Int32 offset = 0x00000000;
            Int32 testAddr = 0x0032EE36;
            string good = "F680";
            while (!found)
            {
                string tester = BytesToHex(memory.ReadMemory(testAddr + offset, 2));
                if (tester == "Service not started. Waiting for PCSX2")
                {
                    break;
                }
                else if (tester == good)
                {
                    found = true;
                }
                else
                {
                    offset += 0x10000000;
                }
            }
            ADDRESS_OFFSET = offset;
        }

        private void CheckPCOffset()
        {
            Int32 testAddr = 0x009AA376 - 0x1000;
            string good = "F680";
            string tester = BytesToHex(memory.ReadMemory(testAddr, 2));
            if (tester == good)
            {
                ADDRESS_OFFSET = -0x1000;
            }
        }

        private void FinishSetupPC(bool PCSX2, Int32 Now, Int32 Save, Int32 Sys3, Int32 Bt10, Int32 BtlEnd, Int32 Slot1)
        {
            //PC needs some slight changing to make sure auto-detect works
            //delay continuing for a short time to avoid connecting too early
            //int Delay = 10000;

            //if auto-detect isn't enabled then we don't wait. 
            //if (!AutoDetectOption.IsChecked)
            //    Delay = 0;

            //await Task.Delay(Delay);
            try
            {
                CheckPCOffset();
            }
            catch (Win32Exception)
            {
                memory = null;
                MessageBox.Show("Unable to access KH2FM try running KHTracker as admin");
                //isWorking = false;
                return;
            }
            catch
            {
                memory = null;
                MessageBox.Show("Error connecting to KH2FM");
                //isWorking = false;
                return;
            }

            //change connection icon visual and start final setup
            Connect.Visibility = Visibility.Collapsed;
            Connect2.Source = data.AD_PC;
            Connect2.Visibility = Visibility.Visible;
            FinishSetup(PCSX2, Now, Save, Sys3, Bt10, BtlEnd, Slot1);
        }

        private void FinishSetup(bool PCSX2, Int32 Now, Int32 Save, Int32 Sys3, Int32 Bt10, Int32 BtlEnd, Int32 Slot1)
        {
            #region Add ICs

            importantChecks = new List<ImportantCheck>();
            importantChecks.Add(highJump = new Ability(memory, Save + 0x25CE, ADDRESS_OFFSET, 93, "HighJump"));
            importantChecks.Add(quickRun = new Ability(memory, Save + 0x25D0, ADDRESS_OFFSET, 97, "QuickRun"));
            importantChecks.Add(dodgeRoll = new Ability(memory, Save + 0x25D2, ADDRESS_OFFSET, 563, "DodgeRoll"));
            importantChecks.Add(aerialDodge = new Ability(memory, Save + 0x25D4, ADDRESS_OFFSET, 101, "AerialDodge"));
            importantChecks.Add(glide = new Ability(memory, Save + 0x25D6, ADDRESS_OFFSET, 105, "Glide"));

            importantChecks.Add(secondChance = new Ability(memory, Save + 0x2544, ADDRESS_OFFSET, "SecondChance", Save));
            importantChecks.Add(onceMore = new Ability(memory, Save + 0x2544, ADDRESS_OFFSET, "OnceMore", Save));

            importantChecks.Add(valor = new DriveForm(memory, Save + 0x36C0, ADDRESS_OFFSET, 1, Save + 0x32F6, Save + 0x06B2, "Valor"));
            importantChecks.Add(wisdom = new DriveForm(memory, Save + 0x36C0, ADDRESS_OFFSET, 2, Save + 0x332E, "Wisdom"));
            importantChecks.Add(limit = new DriveForm(memory, Save + 0x36CA, ADDRESS_OFFSET, 3, Save + 0x3366, "Limit"));
            importantChecks.Add(master = new DriveForm(memory, Save + 0x36C0, ADDRESS_OFFSET, 6, Save + 0x339E, "Master"));
            importantChecks.Add(final = new DriveForm(memory, Save + 0x36C0, ADDRESS_OFFSET, 4, Save + 0x33D6, "Final"));
            importantChecks.Add(anti = new DriveForm(memory, Save + 0x36C0, ADDRESS_OFFSET, 5, Save + 0x340C, "Anti"));

            int fireCount = fire != null ? fire.Level : 0;
            int blizzardCount = blizzard != null ? blizzard.Level : 0;
            int thunderCount = thunder != null ? thunder.Level : 0;
            int cureCount = cure != null ? cure.Level : 0;
            int magnetCount = magnet != null ? magnet.Level : 0;
            int reflectCount = reflect != null ? reflect.Level : 0;

            importantChecks.Add(fire = new Magic(memory, Save + 0x3594, Save + 0x1CF2, ADDRESS_OFFSET, "Fire"));
            importantChecks.Add(blizzard = new Magic(memory, Save + 0x3595, Save + 0x1CF3, ADDRESS_OFFSET, "Blizzard"));
            importantChecks.Add(thunder = new Magic(memory, Save + 0x3596, Save + 0x1CF4, ADDRESS_OFFSET, "Thunder"));
            importantChecks.Add(cure = new Magic(memory, Save + 0x3597, Save + 0x1CF5, ADDRESS_OFFSET, "Cure"));
            importantChecks.Add(magnet = new Magic(memory, Save + 0x35CF, Save + 0x1CF6, ADDRESS_OFFSET, "Magnet"));
            importantChecks.Add(reflect = new Magic(memory, Save + 0x35D0, Save + 0x1CF7, ADDRESS_OFFSET, "Reflect"));

            fire.Level = fireCount;
            blizzard.Level = blizzardCount;
            thunder.Level = thunderCount;
            cure.Level = cureCount;
            magnet.Level = magnetCount;
            reflect.Level = reflectCount;

            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C4, ADDRESS_OFFSET, 6, "Report1"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C4, ADDRESS_OFFSET, 7, "Report2"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C5, ADDRESS_OFFSET, 0, "Report3"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C5, ADDRESS_OFFSET, 1, "Report4"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C5, ADDRESS_OFFSET, 2, "Report5"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C5, ADDRESS_OFFSET, 3, "Report6"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C5, ADDRESS_OFFSET, 4, "Report7"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C5, ADDRESS_OFFSET, 5, "Report8"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C5, ADDRESS_OFFSET, 6, "Report9"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C5, ADDRESS_OFFSET, 7, "Report10"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C6, ADDRESS_OFFSET, 0, "Report11"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C6, ADDRESS_OFFSET, 1, "Report12"));
            importantChecks.Add(reportItem = new Report(memory, Save + 0x36C6, ADDRESS_OFFSET, 2, "Report13"));

            importantChecks.Add(charmItem = new Summon(memory, Save + 0x36C0, ADDRESS_OFFSET, 3, "Baseball"));
            importantChecks.Add(charmItem = new Summon(memory, Save + 0x36C0, ADDRESS_OFFSET, 0, "Ukulele"));
            importantChecks.Add(charmItem = new Summon(memory, Save + 0x36C4, ADDRESS_OFFSET, 4, "Lamp"));
            importantChecks.Add(charmItem = new Summon(memory, Save + 0x36C4, ADDRESS_OFFSET, 5, "Feather"));

            importantChecks.Add(proofItem = new Proof(memory, Save + 0x3694, ADDRESS_OFFSET, "PromiseCharm"));
            importantChecks.Add(proofItem = new Proof(memory, Save + 0x36B4, ADDRESS_OFFSET, "Peace"));
            importantChecks.Add(proofItem = new Proof(memory, Save + 0x36B3, ADDRESS_OFFSET, "Nonexistence"));
            importantChecks.Add(proofItem = new Proof(memory, Save + 0x36B2, ADDRESS_OFFSET, "Connection"));

            importantChecks.Add(visitItem = new Visit(memory, Save + 0x35AE, ADDRESS_OFFSET, "AuronWep"));
            importantChecks.Add(visitItem = new Visit(memory, Save + 0x35AF, ADDRESS_OFFSET, "MulanWep"));
            importantChecks.Add(visitItem = new Visit(memory, Save + 0x35B3, ADDRESS_OFFSET, "BeastWep"));
            importantChecks.Add(visitItem = new Visit(memory, Save + 0x35B4, ADDRESS_OFFSET, "JackWep"));
            importantChecks.Add(visitItem = new Visit(memory, Save + 0x35B5, ADDRESS_OFFSET, "SimbaWep"));
            importantChecks.Add(visitItem = new Visit(memory, Save + 0x35B6, ADDRESS_OFFSET, "SparrowWep"));
            importantChecks.Add(visitItem = new Visit(memory, Save + 0x35C0, ADDRESS_OFFSET, "AladdinWep"));
            importantChecks.Add(visitItem = new Visit(memory, Save + 0x35C2, ADDRESS_OFFSET, "TronWep"));
            importantChecks.Add(visitItem = new Visit(memory, Save + 0x3643, ADDRESS_OFFSET, "MembershipCard"));
            importantChecks.Add(visitItem = new Visit(memory, Save + 0x3649, ADDRESS_OFFSET, "IceCream"));
            importantChecks.Add(visitItem = new Visit(memory, Save + 0x364A, ADDRESS_OFFSET, "Picture"));

            importantChecks.Add(extraItem = new Extra(memory, Save + 0x3696, ADDRESS_OFFSET, "HadesCup"));
            importantChecks.Add(extraItem = new Extra(memory, Save + 0x3644, ADDRESS_OFFSET, "OlympusStone"));
            importantChecks.Add(extraItem = new Extra(memory, Save + 0x365F, ADDRESS_OFFSET, "UnknownDisk"));
            importantChecks.Add(extraItem = new Extra(memory, Save + 0x363C, ADDRESS_OFFSET, "MunnyPouch1"));
            importantChecks.Add(extraItem = new Extra(memory, Save + 0x3695, ADDRESS_OFFSET, "MunnyPouch2"));

            int count = pages != null ? pages.Quantity : 0;
            importantChecks.Add(pages = new TornPage(memory, Save + 0x3598, ADDRESS_OFFSET, "TornPage"));
            pages.Quantity = count;

            #endregion

            if (PCSX2)
                world = new World(memory, ADDRESS_OFFSET, Now, 0x00351EC8, Save + 0x1CFF);
            else
                world = new World(memory, ADDRESS_OFFSET, Now, BtlEnd + 0x820, Save + 0x1CFF);

            stats = new Stats(memory, ADDRESS_OFFSET, Save + 0x24FE, Slot1 + 0x188, Save + 0x3524, Save + 0x3700);
            rewards = new Rewards(memory, ADDRESS_OFFSET, Bt10);

            //forcedfinal = flase;
            checkEveryCheck = new CheckEveryCheck(memory, ADDRESS_OFFSET, Save, Sys3, Bt10, world, stats, rewards, valor, wisdom, limit, master, final);

            // set stat info visibiliy
            Level.Visibility = Visibility.Visible;
            Strength.Visibility = Visibility.Visible;
            Magic.Visibility = Visibility.Visible;
            Defense.Visibility = Visibility.Visible;

            if (FormsGrowthOption.IsChecked)
                FormRow.Height = new GridLength(0.5, GridUnitType.Star);

            //levelcheck visibility
            NextLevelDisplay();
            DeathCounterDisplay();
            SetBindings();
            SetTimer();
            OnTimedEvent(null, null);
        }

        private void SetTimer()
        {
            if (aTimer != null)
                aTimer.Stop();

            aTimer = new DispatcherTimer();
            aTimer.Tick += OnTimedEvent;
            aTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            aTimer.Start();
        }

        ///
        /// Autotracking general
        ///

        private void OnTimedEvent(object sender, EventArgs e)
        {
            previousChecks.Clear();
            previousChecks.AddRange(newChecks);
            newChecks.Clear();

            try
            {
                stats.UpdateMemory();        //updatestats
                world.UpdateMemory();        //current world
                HighlightWorld(world);
                UpdateStatValues();          //set stat values
                UpdateWorldProgress(world);  //progression update
                UpdateFormProgression();
                DeathCheck(pcsx2tracking);   //update deathcounter
                LevelsProgressionBonus();
                DrivesProgressionBonus();

                if (data.mode == Mode.DAHints || data.ScoreMode)
                {
                    UpdatePointScore(0); //update score
                    GetBoss(world);
                }

                importantChecks.ForEach(delegate (ImportantCheck importantCheck)
                {
                    importantCheck.UpdateMemory();
                });

                #region For Debugging
                //Modified to only update if any of these actually change instead of updating every tick
                //temp[0] = world.roomNumber;
                //temp[1] = world.worldNum;
                //temp[2] = world.eventID1;
                //temp[3] = world.eventID2;
                //temp[4] = world.eventID3;
                //temp[5] = world.eventComplete;
                //temp[6] = world.cupRound;
                //if (!Enumerable.SequenceEqual(temp, tempPre))
                //{
                //    Console.WriteLine("world num = " + world.worldNum);
                //    Console.WriteLine("room num  = " + world.roomNumber);
                //    Console.WriteLine("event id1 = " + world.eventID1);
                //    Console.WriteLine("event id2 = " + world.eventID2);
                //    Console.WriteLine("event id3 = " + world.eventID3);
                //    Console.WriteLine("event cpl = " + world.eventComplete);
                //    Console.WriteLine("Cup Round = " + world.cupRound);
                //    tempPre[0] = temp[0];
                //    tempPre[1] = temp[1];
                //    tempPre[2] = temp[2];
                //    tempPre[3] = temp[3];
                //    tempPre[4] = temp[4];
                //    tempPre[5] = temp[5];
                //    tempPre[6] = temp[6];
                //}

                //string cntrl = BytesToHex(memory.ReadMemory(0x2A148E8, 1)); //sora controlable
                //Console.WriteLine(cntrl);
                #endregion
            }
            catch
            {
                aTimer.Stop();
                //isWorking = false;
            
                //if (AutoDetectOption.IsChecked)
                //{
                //    Connect.Visibility = Visibility.Visible;
                //    Connect2.Visibility = Visibility.Collapsed;
                //    SetAutoDetectTimer();
                //}
                //else
                //{
                    Connect.Visibility = Visibility.Collapsed;
                    Connect2.Visibility = Visibility.Visible;
                    Connect2.Source = data.AD_Cross;
                    MessageBox.Show("KH2FM has exited. Stopping Auto Tracker.");
                //}
            
                return;
            }

            UpdateCollectedItems();
            DetermineItemLocations();
        }

        private bool CheckSynthPuzzle(bool ps2)
        {
            if (ps2)
            {
                //reminder: FFFF = unloaded)
                string Jounal = BytesToHex(memory.ReadMemory(0x035F144 + ADDRESS_OFFSET, 2)); //in journal
                //reminder: FF = none | 01 = save menu | 03 = load menu | 05 = moogle | 07 = item popup | 08 = pause menu (cutscene/fight) | 0A = pause Menu (normal)
                string menu = BytesToHex(memory.ReadMemory(0x035F2EC + ADDRESS_OFFSET, 2)); //in a menu

                if ((Jounal == "FFFF" && menu == "0500") || (Jounal != "FFFF" && menu == "0A00")) // in moogle shop / in puzzle menu
                {
                    return true;
                }
                return false;
            }
            else
            {
                string Jounal = BytesToHex(memory.ReadMemory(0x741230, 2)); //in journal
                //reminder: FF = none | 01 = save menu | 03 = load menu | 05 = moogle | 07 = item popup | 08 = pause menu (cutscene/fight) | 0A = pause Menu (normal)
                string menu = BytesToHex(memory.ReadMemory(0x741320, 2)); //in a menu

                if ((Jounal == "FFFF" && menu == "0500") || (Jounal != "FFFF" && menu == "0A00")) // in moogle shop / in puzzle menu
                {
                    return true;
                }
                return false;
            }
        }

        //private bool CheckTornPage(Item item)
        //{
        //    //return true and track item for anything that isn't a torn page
        //    if (!item.Name.StartsWith("TornPage"))
        //        return true;
        //
        //    int Tracked = WorldGrid.Real_Pages; //current number of pages tracked to any of the world grids
        //    int Inventory = memory.ReadMemory(ADDRESS_OFFSET + 0x09A70B0 + 0x3598, 1)[0]; //number of pages currently in sora's inventory
        //    int Used = 0; //number of torn pages used so far in 100 AW
        //
        //    //don't try tracking a torn page if we already tracked 5
        //    //as there should only ever be 5 total under normal means.
        //    if(Tracked >= 5)
        //        return false;
        //
        //    //note: Save = 0x09A70B0;
        //    //check current 100 AW story flags to see what pages have been used already.
        //    if (new BitArray(memory.ReadMemory(ADDRESS_OFFSET + 0x09A70B0 + 0x1DB1, 1))[1]) //page 1 used flag
        //        Used = 1;
        //    if (new BitArray(memory.ReadMemory(ADDRESS_OFFSET + 0x09A70B0 + 0x1DB1, 1))[1]) //page 2 used flag
        //        Used = 2;
        //    if (new BitArray(memory.ReadMemory(ADDRESS_OFFSET + 0x09A70B0 + 0x1DB1, 1))[1]) //page 3 used flag
        //        Used = 3;
        //    if (new BitArray(memory.ReadMemory(ADDRESS_OFFSET + 0x09A70B0 + 0x1DB1, 1))[1]) //page 4 used flag
        //        Used = 4;
        //
        //    //if number of torn pages used + current number of pages in sora's inventory
        //    //are equal to the current number of pages tracked, then don't track anything.
        //    if (Used + Inventory == Tracked)
        //        return false;
        //
        //    return true;
        //}

        private void DeathCheck(bool ps2)
        {
            //Note: 04 = dying, 05 = continue screen.
            //note: if i try tracking a death when pausecheck is "0400" then that should give a
            //more accurate death count in the event that continue is selected too fast (i hope)

            string PauseCheck;

            if (ps2)
            {
                PauseCheck = BytesToHex(memory.ReadMemory(0x0347E08 + ADDRESS_OFFSET, 2));
            }
            else
            {
                PauseCheck = BytesToHex(memory.ReadMemory(0xAB9078, 2));
            }

            //if oncontinue is try true then we want to check if the values for sora is currently dying or on continue screen.
            //we need to chck this to prevent the counter rapidly counting up every frame adnd such
            if (onContinue)
            {
                if (PauseCheck == "0400" || PauseCheck == "0500")
                    return;
                else
                    onContinue = false;
            }

            // if sora is currently dying or on the continue screen
            // then increase death count and set oncontinue
            if (PauseCheck == "0400" || PauseCheck == "0500")
            {
                DeathCounter++;
                onContinue = true;
            }

            DeathValue.Text = DeathCounter.ToString();
        }

        private void UpdateStatValues()
        {
            // we don't need bindings anymore (i think) so use this instead

            //Main window
            //Stats
            stats.SetNextLevelCheck(stats.Level);
            LevelValue.Text = stats.Level.ToString();
            StrengthValue.Text = stats.Strength.ToString();
            MagicValue.Text = stats.Magic.ToString();
            DefenseValue.Text = stats.Defense.ToString();
            //forms
            ValorLevel.Text = valor.VisualLevel.ToString();
            WisdomLevel.Text = wisdom.VisualLevel.ToString();
            LimitLevel.Text = limit.VisualLevel.ToString();
            MasterLevel.Text = master.VisualLevel.ToString();
            FinalLevel.Text = final.VisualLevel.ToString();
            //growth
            HighJumpLevel.Text = highJump.Level.ToString();
            QuickRunLevel.Text = quickRun.Level.ToString();
            DodgeRollLevel.Text = dodgeRoll.Level.ToString();
            AerialDodgeLevel.Text = aerialDodge.Level.ToString();
            GlideLevel.Text = glide.Level.ToString();
        }

        private void TrackItem(string itemName, WorldGrid world)
        {
            Grid ItemRow = null;
            try //try getting itemrow grid from dictionary
            {
                ItemRow = data.Items[itemName].Item2;
            }
            catch //if item is not from pool (growth) then log the item and return
            {
                if (App.logger != null)
                    App.logger.Record(itemName + " tracked");
                return;
            }

            //do a check in the report handler to actually make sure reports don't
            //track to the wrong place in the case of mismatched seeds/hints
            if (ItemRow.FindName(itemName) is Item item && item.IsVisible)
            {
                bool validItem = world.ReportHandler(item);

                if (validItem)
                {
                    //string LogText;
                    ////check torn pages
                    //if (CheckTornPage(item) == false)
                    //{
                    //    LogText = "Attempted to track a page past 5?";
                    //}
                    //else
                    //{
                    //    
                    //    LogText = item.Name + " tracked";
                    //}

                    world.Add_Item(item);
                    if (App.logger != null)
                        App.logger.Record(item.Name + " tracked");


                }
            }
        }

        private void TrackQuantities()
        {
            while (fire.Level > fireLevel)
            {
                ++fireLevel;
                Magic magic = new Magic(null, 0, 0, 0, "Fire" + fireLevel.ToString());
                newChecks.Add(magic);
                collectedChecks.Add(magic);
            }
            while (blizzard.Level > blizzardLevel)
            {
                ++blizzardLevel;
                Magic magic = new Magic(null, 0, 0, 0, "Blizzard" + blizzardLevel.ToString());
                newChecks.Add(magic);
                collectedChecks.Add(magic);
            }
            while (thunder.Level > thunderLevel)
            {
                ++thunderLevel;
                Magic magic = new Magic(null, 0, 0, 0, "Thunder" + thunderLevel.ToString());
                newChecks.Add(magic);
                collectedChecks.Add(magic);
            }
            while (cure.Level > cureLevel)
            {
                ++cureLevel;
                Magic magic = new Magic(null, 0, 0, 0, "Cure" + cureLevel.ToString());
                newChecks.Add(magic);
                collectedChecks.Add(magic);
            }
            while (reflect.Level > reflectLevel)
            {
                ++reflectLevel;
                Magic magic = new Magic(null, 0, 0, 0, "Reflect" + reflectLevel.ToString());
                newChecks.Add(magic);
                collectedChecks.Add(magic);
            }
            while (magnet.Level > magnetLevel)
            {
                ++magnetLevel;
                Magic magic = new Magic(null, 0, 0, 0, "Magnet" + magnetLevel.ToString());
                newChecks.Add(magic);
                collectedChecks.Add(magic);
            }
            while (pages.Quantity > tornPageCount)
            {
                ++tornPageCount;
                TornPage page = new TornPage(null, 0, 0, "TornPage" + tornPageCount.ToString());
                newChecks.Add(page);
                collectedChecks.Add(page);
            }
        }

        //progression hints level bonus
        private void LevelsProgressionBonus()
        {
            //if sora's current level is great than the max specified level (usually 50), then do nothing
            if (stats.Level > (data.Levels_ProgressionValues.Count * 10) || !data.UsingProgressionHints)
                return;

            //every 10 levels, reward the player the progression points for that part
            while (stats.Level > data.NextLevelMilestone)
            {
                data.NextLevelMilestone += 10;
                AddProgressionPoints(data.Levels_ProgressionValues[data.LevelsPreviousIndex++]);
            }
        }

        private void DrivesProgressionBonus()
        {
            if (!data.UsingProgressionHints)
                return;

            //check valor
            while (valor.Level > data.DriveLevels[0])
            {
                //Console.WriteLine("data.DriveLevels[0] Current = " + data.DriveLevels[0]);
                //Console.WriteLine("data.Drives_ProgressionValues[data.DriveLevels[0]] = " + data.Drives_ProgressionValues[data.DriveLevels[0] - 1]);
                AddProgressionPoints(data.Drives_ProgressionValues[data.DriveLevels[0] - 1]);
                data.DriveLevels[0]++;
            }
            while (wisdom.Level > data.DriveLevels[1])
            {
                AddProgressionPoints(data.Drives_ProgressionValues[data.DriveLevels[1] - 1]);
                data.DriveLevels[1]++;
            }
            while (limit.Level > data.DriveLevels[2])
            {
                AddProgressionPoints(data.Drives_ProgressionValues[data.DriveLevels[2] - 1]);
                data.DriveLevels[2]++;
            }
            while (master.Level > data.DriveLevels[3])
            {
                AddProgressionPoints(data.Drives_ProgressionValues[data.DriveLevels[3] - 1]);
                data.DriveLevels[3]++;
            }
            while (final.Level > data.DriveLevels[4])
            {
                AddProgressionPoints(data.Drives_ProgressionValues[data.DriveLevels[4] - 1]);
                data.DriveLevels[4]++;
            }
        }

        private void UpdateWorldProgress(World world)
        {
            if (world.worldName == "DestinyIsland" || world.worldName == "Unknown")
                return;

            //check for valid progression Content Controls first
            ContentControl progressionM = data.WorldsData[world.worldName].progression;

            //Get current icon prefixes (simple, game, or custom icons)
            bool OldToggled = Properties.Settings.Default.OldProg;
            bool CustomToggled = Properties.Settings.Default.CustomIcons;
            string Prog = "Min-"; //Default
            if (OldToggled)
                Prog = "Old-";
            if (CustomProgFound && CustomToggled)
                Prog = "Cus-";

            //progression defaults
            int curProg = data.WorldsData[world.worldName].progress; //current world progress int
            string curKey; //current world progression icon name
            string curDes;

            //get current world progress key
            switch (world.worldName)
            {
                case "SimulatedTwilightTown":
                    switch (world.roomNumber) //check based on room number now, then based on events in each room
                    {
                        case 1:
                            if ((world.eventID3 == 56 || world.eventID3 == 55) && data.WorldsData[world.worldName].progress == 0) // Roxas' Room (Day 1)/(Day 6)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 8:
                            if (world.eventID1 == 110 || world.eventID1 == 111) // Get Ollete Munny Pouch (min/max munny cutscenes)
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 34:
                            if (world.eventID1 == 157 && world.eventComplete == 1) // Twilight Thorn finish
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 5:
                            if (world.eventID1 == 87 && world.eventComplete == 1) // Axel 1 Finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            if (world.eventID1 == 88 && world.eventComplete == 1) // Setzer finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 21:
                            if (world.eventID3 == 1) // Mansion: Computer Room
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            break;
                        case 20:
                            if (world.eventID1 == 137 && world.eventComplete == 1) // Axel 2 finish
                                UpdateProgressionPoints(world.worldName,  curProg = 7);
                            break;
                        default: //if not in any of the above rooms then just leave
                            return;
                    }
                    break;
                case "TwilightTown":
                    switch (world.roomNumber)
                    {
                        case 9:
                            if (world.eventID3 == 117 && data.WorldsData[world.worldName].progress == 0) // Roxas' Room (Day 1)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 8:
                            if (world.eventID3 == 108 && world.eventComplete == 1) // Station Nobodies
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 27:
                            if (world.eventID3 == 4) // Yen Sid after new clothes
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 4:
                            if (world.eventID1 == 80 && world.eventComplete == 1) // Sandlot finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 41:
                            if (world.eventID1 == 186 && world.eventComplete == 1) // Mansion fight finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 40:
                            if (world.eventID1 == 161 && world.eventComplete == 1) // Betwixt and Between finish
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            break;
                        case 20:
                            if (world.eventID1 == 213 && world.eventComplete == 1) // Data Axel finish
                            {
                                UpdateProgressionPoints(world.worldName,  curProg = 7);
                                data.TT_ProgressionValues[7 - 1] = 0;
                            }
                            break;
                        default:
                            return;
                    }
                    break;
                case "HollowBastion":
                    switch (world.roomNumber)
                    {
                        case 0:
                        case 10:
                            if ((world.eventID3 == 1 || world.eventID3 == 2) && data.WorldsData[world.worldName].progress == 0) // Villain's Vale (HB1)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 8:
                            if (world.eventID1 == 52 && world.eventComplete == 1) // Bailey finish
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 5:
                            if (world.eventID3 == 20) // Ansem Study post Computer
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 20:
                            if (world.eventID1 == 86 && world.eventComplete == 1) // Corridor finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 18:
                            if (world.eventID1 == 73 && world.eventComplete == 1) // Dancers finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 4:
                            if (world.eventID1 == 55 && world.eventComplete == 1) // HB Demyx finish
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            else if (world.eventID1 == 114 && world.eventComplete == 1) // Data Demyx finish
                            {
                                if (curProg == 9)
                                    curProg = 11; //data demyx + sephi finished
                                else if (curProg != 11)
                                    curProg = 10;
                                UpdateProgressionPoints(world.worldName,  10);
                                data.HB_ProgressionValues[10 - 1] = 0;
                            }
                            break;
                        case 16:
                            if (world.eventID1 == 65 && world.eventComplete == 1) // FF Cloud finish
                                UpdateProgressionPoints(world.worldName,  curProg = 7);
                            break;
                        case 17:
                            if (world.eventID1 == 66 && world.eventComplete == 1) // 1k Heartless finish
                                UpdateProgressionPoints(world.worldName,  curProg = 8);
                            break;
                        case 1:
                            if (world.eventID1 == 75 && world.eventComplete == 1) // Sephiroth finish
                            {
                                if (curProg == 10)
                                    curProg = 11; //data demyx + sephi finished
                                else if (curProg != 11)
                                    curProg = 9;
                                UpdateProgressionPoints(world.worldName,  9);
                            }
                            break;
                        //CoR
                        case 21:
                            if ((world.eventID3 == 1 || world.eventID3 == 2) && data.WorldsData["GoA"].progress == 0) //Enter CoR
                            {
                                curKey = data.ProgressKeys["GoA"][1];
                                GoAProgression.SetResourceReference(ContentProperty, Prog + curKey);
                                data.WorldsData["GoA"].progress = 1;
                                UpdateProgressionPoints("CavernofRemembrance", 1);
                                return;
                            }
                            break;
                        case 22:
                            if (world.eventID3 == 1 && data.WorldsData["GoA"].progress <= 1 && world.eventComplete == 1) //valves after skip
                            {
                                curKey = data.ProgressKeys["GoA"][5];
                                GoAProgression.SetResourceReference(ContentProperty, Prog + curKey);
                                data.WorldsData["GoA"].progress = 5;
                                UpdateProgressionPoints("CavernofRemembrance", 3);
                                return;
                            }
                            break;
                        case 24:
                            if (world.eventID3 == 1 && world.eventComplete == 1) //first fight
                            {
                                curKey = data.ProgressKeys["GoA"][2];
                                GoAProgression.SetResourceReference(ContentProperty, Prog + curKey);
                                data.WorldsData["GoA"].progress = 2;
                                UpdateProgressionPoints("CavernofRemembrance", 2);
                                return;
                            }
                            if (world.eventID3 == 2 && world.eventComplete == 1) //second fight
                            {
                                curKey = data.ProgressKeys["GoA"][3];
                                GoAProgression.SetResourceReference(ContentProperty, Prog + curKey);
                                data.WorldsData["GoA"].progress = 3;
                                UpdateProgressionPoints("CavernofRemembrance", 4);
                                return;
                            }
                            break;
                        case 25:
                            if (world.eventID3 == 3 && world.eventComplete == 1) //transport
                            {
                                curKey = data.ProgressKeys["GoA"][4];
                                GoAProgression.SetResourceReference(ContentProperty, Prog + curKey);
                                data.WorldsData["GoA"].progress = 4;
                                UpdateProgressionPoints("CavernofRemembrance", 5);
                                return;
                            }
                            break;
                        default:
                            return;
                    }
                    break;
                case "BeastsCastle":
                    switch (world.roomNumber)
                    {
                        case 0:
                        case 2:
                            if ((world.eventID3 == 1 || world.eventID3 == 10) && data.WorldsData[world.worldName].progress == 0) // Entrance Hall (BC1)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 11:
                            if (world.eventID1 == 72 && world.eventComplete == 1) // Thresholder finish
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 3:
                            if (world.eventID1 == 69 && world.eventComplete == 1) // Beast finish
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 5:
                            if (world.eventID1 == 79 && world.eventComplete == 1) // Dark Thorn finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 4:
                            if (world.eventID1 == 74 && world.eventComplete == 1) // Dragoons finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 15:
                            if (world.eventID1 == 82 && world.eventComplete == 1) // Xaldin finish
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            else if (world.eventID1 == 97 && world.eventComplete == 1) // Data Xaldin finish
                            {
                                UpdateProgressionPoints(world.worldName,  curProg = 7);
                                data.BC_ProgressionValues[7 - 1] = 0;
                            }
                            break;
                        default:
                            return;
                    }
                    break;
                case "OlympusColiseum":
                    switch (world.roomNumber)
                    {
                        case 3:
                            if ((world.eventID3 == 1 || world.eventID3 == 12) && data.WorldsData[world.worldName].progress == 0) // The Coliseum (OC1) | Underworld Entrance (OC2)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 7:
                            if (world.eventID1 == 114 && world.eventComplete == 1) // Cerberus finish
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 0:
                            if ((world.eventID3 == 1 || world.eventID3 == 12) && data.WorldsData[world.worldName].progress == 0) // (reverse rando)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            if (world.eventID1 == 141 && world.eventComplete == 1) // Urns finish
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 17:
                            if (world.eventID1 == 123 && world.eventComplete == 1) // OC Demyx finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 8:
                            if (world.eventID1 == 116 && world.eventComplete == 1) // OC Pete finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 18:
                            if (world.eventID1 == 171 && world.eventComplete == 1) // Hydra finish
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            break;
                        case 6:
                            if (world.eventID1 == 126 && world.eventComplete == 1) // Auron Statue fight finish
                                UpdateProgressionPoints(world.worldName,  curProg = 7);
                            break;
                        case 19:
                            if (world.roomNumber == 19 && world.eventID1 == 202 && world.eventComplete == 1) // Hades finish
                                UpdateProgressionPoints(world.worldName,  curProg = 8);
                            break;
                        case 34:
                            if ((world.eventID1 == 151) && world.eventComplete == 1) // AS Zexion finish
                                UpdateProgressionPoints(world.worldName,  curProg = 9);
                            else if ((world.eventID1 == 152) && world.eventComplete == 1) // Data Zexion finish
                            {
                                UpdateProgressionPoints(world.worldName, 10);
                                data.OC_ProgressionValues[10 - 1] = 0;
                            }
                            break;
                        default:
                            return;
                    }
                    break;
                case "Agrabah":
                    switch (world.roomNumber)
                    {
                        case 0:
                        case 4:
                            if ((world.eventID3 == 1 || world.eventID3 == 10) && data.WorldsData[world.worldName].progress == 0) // Agrabah (Ag1) || The Vault (Ag2)
                                UpdateProgressionPoints(world.worldName, curProg = 1);
                            break;
                        case 9:
                            if (world.eventID1 == 2 && world.eventComplete == 1) // Abu finish
                                UpdateProgressionPoints(world.worldName, curProg = 2);
                            break;
                        case 13:
                            if (world.eventID1 == 79 && world.eventComplete == 1) // Chasm fight finish
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 10:
                            if (world.eventID1 == 58 && world.eventComplete == 1) // Treasure Room finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 3:
                            if (world.eventID1 == 59 && world.eventComplete == 1) // Lords finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 14:
                            if (world.eventID1 == 100 && world.eventComplete == 1) // Carpet finish
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            break;
                        case 5:
                            if (world.eventID1 == 62 && world.eventComplete == 1) // Genie Jafar finish
                                UpdateProgressionPoints(world.worldName,  curProg = 7);
                            break;
                        case 33:
                            if ((world.eventID1 == 142) && world.eventComplete == 1) // AS Lexaeus finish
                                UpdateProgressionPoints(world.worldName,  curProg = 8);
                            else if ((world.eventID1 == 147) && world.eventComplete == 1) // Data Lexaeus
                            {
                                UpdateProgressionPoints(world.worldName,  9);
                                data.AG_ProgressionValues[9 - 1] = 0;
                            }
                            break;
                        default:
                            return;
                    }
                    break;
                case "LandofDragons":
                    switch (world.roomNumber)
                    {
                        case 0:
                        case 12:
                            if ((world.eventID3 == 1 || world.eventID3 == 10) && data.WorldsData[world.worldName].progress == 0) // Bamboo Grove (LoD1)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 1:
                            if (world.eventID1 == 70 && world.eventComplete == 1) // Mission 3 (Search) finish
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 3:
                            if (world.eventID1 == 71 && world.eventComplete == 1) // Mountain Climb finish
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 5:
                            if (world.eventID1 == 72 && world.eventComplete == 1) // Cave finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 7:
                            if (world.eventID1 == 73 && world.eventComplete == 1) // Summit finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 9:
                            if (world.eventID1 == 75 && world.eventComplete == 1) // Shan Yu finish
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            break;
                        case 10:
                            if (world.eventID1 == 78 && world.eventComplete == 1) // Antechamber fight finish
                                UpdateProgressionPoints(world.worldName,  curProg = 7);
                            break;
                        case 8:
                            if (world.eventID1 == 79 && world.eventComplete == 1) // Storm Rider finish
                                UpdateProgressionPoints(world.worldName,  curProg = 8);
                            break;
                        default:
                            return;
                    }
                    break;
                case "HundredAcreWood":
                    switch (world.roomNumber)
                    {
                        case 2:
                            if ((world.eventID3 == 1 || world.eventID3 == 21 || world.eventID3 == 22) && data.WorldsData[world.worldName].progress == 0) // Pooh's house
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 4:
                            if (world.eventID3 == 4) // Piglet's house
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 3:
                            if (world.eventID3 == 4) // Rabbit's house
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 5:
                            if (world.eventID3 == 4) // Kanga's house
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 9:
                            if (world.eventID3 == 61) // Spooky Cave
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 1:
                            if (world.eventID3 == 52) // Starry Hill
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            break;
                        default:
                            return;
                    }
                    break;
                case "PrideLands":
                    switch (world.roomNumber)
                    {
                        case 4:
                        case 16:
                            if ((world.eventID3 == 1 || world.eventID3 == 10) && data.WorldsData[world.worldName].progress == 0) // Wildebeest Valley (PL1)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 12:
                            if (world.eventID3 == 1) // Oasis after talking to Simba
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 2:
                            if (world.eventID1 == 51 && world.eventComplete == 1) // Hyenas 1 Finish
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 14:
                            if (world.eventID1 == 55 && world.eventComplete == 1) // Scar finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 5:
                            if (world.eventID1 == 57 && world.eventComplete == 1) // Hyenas 2 Finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 15:
                            if (world.eventID1 == 59 && world.eventComplete == 1) // Groundshaker finish
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            break;
                        default:
                            return;
                    }
                    break;
                case "Atlantica":
                    switch (world.roomNumber)
                    {
                        case 2:
                            if (world.eventID1 == 63) // Tutorial
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 9:
                            if (world.eventID1 == 65) // Ursula's Revenge
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 4:
                            if (world.eventID1 == 55) // A New Day is Dawning
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        default:
                            return;
                    }
                    break;
                case "DisneyCastle":
                    switch (world.roomNumber)
                    {
                        case 0:
                            if (world.eventID3 == 22 && data.WorldsData[world.worldName].progress == 0) // Cornerstone Hill (TR) (Audience Chamber has no Evt 0x16)
                                curProg = 0;
                            else if (world.eventID1 == 51 && world.eventComplete == 1) // Minnie Escort finish
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            else if (world.eventID3 == 6) // Windows popup (Audience Chamber has no Evt 0x06)
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 1:
                            if (world.eventID1 == 53 && data.WorldsData[world.worldName].progress == 0) // Library (DC)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            else if (world.eventID1 == 58 && world.eventComplete == 1) // Old Pete finish
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 2:
                            if (world.eventID1 == 52 && world.eventComplete == 1) // Boat Pete finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 3:
                            if (world.eventID1 == 53 && world.eventComplete == 1) // DC Pete finish
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            break;
                        case 38:
                            if ((world.eventID1 == 145 || world.eventID1 == 150) && world.eventComplete == 1) // Marluxia finish
                            {
                                if (curProg == 8)
                                    curProg = 9; //marluxia + LW finished
                                else if (curProg != 9)
                                    curProg = 7;
                                if (world.eventID1 == 145)
                                    UpdateProgressionPoints(world.worldName, 7); // AS
                                else
                                {
                                    UpdateProgressionPoints(world.worldName, 8); // Data
                                    data.DC_ProgressionValues[8 - 1] = 0;
                                }
                            }
                            break;
                        case 7:
                            if (world.eventID1 == 67 && world.eventComplete == 1) // Lingering Will finish
                            {
                                if (curProg == 7)
                                    curProg = 9; //marluxia + LW finished
                                else if (curProg != 9)
                                    curProg = 8;
                                UpdateProgressionPoints(world.worldName,  9);
                                data.DC_ProgressionValues[9 - 1] = 0;
                            }
                            break;
                        default:
                            return;
                    }
                    break;
                case "HalloweenTown":
                    switch (world.roomNumber)
                    {
                        case 1:
                        case 4:
                            if ((world.eventID3 == 1 || world.eventID3 == 10) && data.WorldsData[world.worldName].progress == 0) // Hinterlands (HT1)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 6:
                            if (world.eventID1 == 53 && world.eventComplete == 1) // Candy Cane Lane fight finish
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 3:
                            if (world.eventID1 == 52 && world.eventComplete == 1) // Prison Keeper finish
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 9:
                            if (world.eventID1 == 55 && world.eventComplete == 1) // Oogie Boogie finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 10:
                            if (world.eventID1 == 62 && world.eventComplete == 1) // Children Fight
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            if (world.eventID1 == 63 && world.eventComplete == 1) // Presents minigame
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            break;
                        case 7:
                            if (world.eventID1 == 64 && world.eventComplete == 1) // Experiment finish
                                UpdateProgressionPoints(world.worldName,  curProg = 7);
                            break;
                        case 32:
                            if ((world.eventID1 == 115) && world.eventComplete == 1) // AS Vexen finish
                                UpdateProgressionPoints(world.worldName,  curProg = 8);
                            else if ((world.eventID1 == 146) && world.eventComplete == 1) // Data Vexen finish
                            {
                                UpdateProgressionPoints(world.worldName, 9);
                                data.HT_ProgressionValues[9 - 1] = 0;
                            }
                            break;
                        default:
                            return;
                    }
                    break;
                case "PortRoyal":
                    switch (world.roomNumber)
                    {
                        case 0:
                            if (world.eventID3 == 1 && data.WorldsData[world.worldName].progress == 0) // Rampart (PR1)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 10:
                            if (world.eventID3 == 10 && data.WorldsData[world.worldName].progress == 0) // Treasure Heap (PR2)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            if (world.eventID1 == 60 && world.eventComplete == 1) // Barbossa finish
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            break;
                        case 2:
                            if (world.eventID1 == 55 && world.eventComplete == 1) // Town finish
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 9:
                            if (world.eventID1 == 59 && world.eventComplete == 1) // 1min pirates finish
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 7:
                            if (world.eventID1 == 58 && world.eventComplete == 1) // Medalion fight finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 3:
                            if (world.eventID1 == 56 && world.eventComplete == 1) // barrels finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 18:
                            if (world.eventID1 == 85 && world.eventComplete == 1) // Grim Reaper 1 finish
                                UpdateProgressionPoints(world.worldName,  curProg = 7);
                            break;
                        case 14:
                            if (world.eventID1 == 62 && world.eventComplete == 1) // Gambler finish
                                UpdateProgressionPoints(world.worldName,  curProg = 8);
                            break;
                        case 1:
                            if (world.eventID1 == 54 && world.eventComplete == 1) // Grim Reaper 2 finish
                                UpdateProgressionPoints(world.worldName,  curProg = 9);
                            break;
                        default:
                            return;
                    }
                    break;
                case "SpaceParanoids":
                    switch (world.roomNumber)
                    {
                        case 1:
                            if ((world.eventID3 == 1 || world.eventID3 == 10) && data.WorldsData[world.worldName].progress == 0) // Canyon (SP1)
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 3:
                            if (world.eventID1 == 54 && world.eventComplete == 1) // Screens finish
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            break;
                        case 4:
                            if (world.eventID1 == 55 && world.eventComplete == 1) // Hostile Program finish
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            break;
                        case 7:
                            if (world.eventID1 == 57 && world.eventComplete == 1) // Solar Sailer finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            break;
                        case 9:
                            if (world.eventID1 == 59 && world.eventComplete == 1) // MCP finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            break;
                        case 33:
                            if ((world.eventID1 == 143) && world.eventComplete == 1) // AS Larxene finish
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            else if ((world.eventID1 == 148) && world.eventComplete == 1) // Data Larxene finish
                            {
                                UpdateProgressionPoints(world.worldName,  7);
                                data.SP_ProgressionValues[7 - 1] = 0;
                            }
                            break;
                        default:
                            return;
                    }
                    break;
                case "TWTNW":
                    switch (world.roomNumber)
                    {
                        case 1:
                            if (world.eventID3 == 1) // Alley to Between
                                UpdateProgressionPoints(world.worldName,  curProg = 1);
                            break;
                        case 21:
                            if (world.eventID1 == 65 && world.eventComplete == 1) // Roxas finish
                                UpdateProgressionPoints(world.worldName,  curProg = 2);
                            else if (world.eventID1 == 99 && world.eventComplete == 1) // Data Roxas finish
                            {
                                curKey = data.ProgressKeys["SimulatedTwilightTown"][8];
                                SimulatedTwilightTownProgression.SetResourceReference(ContentProperty, Prog + curKey);
                                data.WorldsData["SimulatedTwilightTown"].progress = 8;
                                UpdateProgressionPoints("SimulatedTwilightTown", 8);
                                data.STT_ProgressionValues[8 - 1] = 0;
                                return;
                            }
                            break;
                        case 10:
                            if (world.eventID1 == 57 && world.eventComplete == 1) // Xigbar finish
                                UpdateProgressionPoints(world.worldName,  curProg = 3);
                            else if (world.eventID1 == 100 && world.eventComplete == 1) // Data Xigbar finish
                            {
                                curKey = data.ProgressKeys["LandofDragons"][9];
                                LandofDragonsProgression.SetResourceReference(ContentProperty, Prog + curKey);
                                data.WorldsData["LandofDragons"].progress = 9;
                                UpdateProgressionPoints("LandofDragons", 9);
                                data.LoD_ProgressionValues[9 - 1] = 0;
                                return;
                            }
                            break;
                        case 14:
                            if (world.eventID1 == 58 && world.eventComplete == 1) // Luxord finish
                                UpdateProgressionPoints(world.worldName,  curProg = 4);
                            else if (world.eventID1 == 101 && world.eventComplete == 1) // Data Luxord finish
                            {
                                curKey = data.ProgressKeys["PortRoyal"][10];
                                PortRoyalProgression.SetResourceReference(ContentProperty, Prog + curKey);
                                data.WorldsData["PortRoyal"].progress = 10;
                                UpdateProgressionPoints("PortRoyal", 10);
                                data.PR_ProgressionValues[10 - 1] = 0;
                                return;
                            }
                            break;
                        case 15:
                            if (world.eventID1 == 56 && world.eventComplete == 1) // Saix finish
                                UpdateProgressionPoints(world.worldName,  curProg = 5);
                            else if (world.eventID1 == 102 && world.eventComplete == 1) // Data Saix finish
                            {
                                curKey = data.ProgressKeys["PrideLands"][7];
                                PrideLandsProgression.SetResourceReference(ContentProperty, Prog + curKey);
                                data.WorldsData["PrideLands"].progress = 7;
                                UpdateProgressionPoints("PrideLands", 7);
                                data.PL_ProgressionValues[7 - 1] = 0;
                                return;
                            }
                            break;
                        case 19:
                            if (world.eventID1 == 59 && world.eventComplete == 1) // Xemnas 1 finish
                                UpdateProgressionPoints(world.worldName,  curProg = 6);
                            break;
                        case 20:
                            if (world.eventID1 == 98 && world.eventComplete == 1) // Data Xemnas finish
                            {
                                UpdateProgressionPoints(world.worldName, curProg = 7);
                                data.TWTNW_ProgressionValues[7 - 1] = 0;
                            }
                            else if (world.eventID1 == 74 && world.eventComplete == 1 && data.revealFinalXemnas) // Regular Final Xemnas finish
                                UpdateProgressionPointsTWTNW(world.worldName);
                            break;
                        default:
                            return;
                    }
                    break;
                case "GoA":
                    if (world.roomNumber == 32)
                    {
                        if (HashGrid.Visibility == Visibility.Visible)
                        {
                            HashGrid.Visibility = Visibility.Collapsed;
                        }
                    }
                    return;
                default: //return if any other world
                    return;
            }

            //made it this far, now just set the progression icon based on the new curProg
            curKey = data.ProgressKeys[world.worldName][curProg];
            curDes = data.ProgressKeys[world.worldName + "Desc"][curProg];
            progressionM.SetResourceReference(ContentProperty, Prog + curKey);
            data.WorldsData[world.worldName].progress = curProg;
            data.WorldsData[world.worldName].progression.ToolTip = curDes;
        }

        // Sometimes level rewards and levels dont update on the same tick
        // Previous tick checks are placed on the current tick with the info of both ticks
        // This way level checks don't get misplaced 
        //Note: apparently the above is completely untrue, but its's not like it currently breaks anything so...
        private void DetermineItemLocations()
        {
            if (previousChecks.Count == 0)
                return;

            // Get rewards between previous level and current level
            List<string> levelRewards = rewards.GetLevelRewards(stats.Weapon)
                .Where(reward => reward.Item1 > stats.previousLevels[0] && reward.Item1 <= stats.Level)
                .Select(reward => reward.Item2).ToList();
            // Get drive rewards between previous level and current level
            List<string> driveRewards = rewards.valorChecks
                .Where(reward => reward.Item1 > valor.previousLevels[0] && reward.Item1 <= valor.Level)
                .Select(reward => reward.Item2).ToList();
            driveRewards.AddRange(rewards.wisdomChecks
                .Where(reward => reward.Item1 > wisdom.previousLevels[0] && reward.Item1 <= wisdom.Level)
                .Select(reward => reward.Item2));
            driveRewards.AddRange(rewards.limitChecks
                .Where(reward => reward.Item1 > limit.previousLevels[0] && reward.Item1 <= limit.Level)
                .Select(reward => reward.Item2));
            driveRewards.AddRange(rewards.masterChecks
                .Where(reward => reward.Item1 > master.previousLevels[0] && reward.Item1 <= master.Level)
                .Select(reward => reward.Item2));
            driveRewards.AddRange(rewards.finalChecks
                .Where(reward => reward.Item1 > final.previousLevels[0] && reward.Item1 <= final.Level)
                .Select(reward => reward.Item2));

            if (stats.Level > stats.previousLevels[0] && App.logger != null)
                App.logger.Record("Levels " + stats.previousLevels[0].ToString() + " to " + stats.Level.ToString());
            if (valor.Level > valor.previousLevels[0] && App.logger != null)
                App.logger.Record("Valor Levels " + valor.previousLevels[0].ToString() + " to " + valor.Level.ToString());
            if (wisdom.Level > wisdom.previousLevels[0] && App.logger != null)
                App.logger.Record("Wisdom Levels " + wisdom.previousLevels[0].ToString() + " to " + wisdom.Level.ToString());
            if (limit.Level > limit.previousLevels[0] && App.logger != null)
                App.logger.Record("Limit Levels " + limit.previousLevels[0].ToString() + " to " + limit.Level.ToString());
            if (master.Level > master.previousLevels[0] && App.logger != null)
                App.logger.Record("Master Levels " + master.previousLevels[0].ToString() + " to " + master.Level.ToString());
            if (final.Level > final.previousLevels[0] && App.logger != null)
                App.logger.Record("Final Levels " + final.previousLevels[0].ToString() + " to " + final.Level.ToString());
            foreach (string str in levelRewards)
            {
                if (App.logger != null)
                    App.logger.Record("Level reward " + str);
            }
            foreach (string str in driveRewards)
            {
                if (App.logger != null)
                    App.logger.Record("Drive reward " + str);
            }

            foreach (ImportantCheck check in previousChecks)
            {
                string count = "";
                // remove magic and torn page count for comparison with item codes and readd to track specific ui copies
                if (check.GetType() == typeof(Magic) || check.GetType() == typeof(TornPage))
                {
                    count = check.Name.Substring(check.Name.Length - 1);
                    check.Name = check.Name.Substring(0, check.Name.Length - 1);
                }

                if (levelRewards.Exists(x => x == check.Name))
                {
                    // add check to levels
                    TrackItem(check.Name + count, SorasHeartGrid);
                    levelRewards.Remove(check.Name);
                }
                else if (driveRewards.Exists(x => x == check.Name))
                {
                    // add check to drives
                    TrackItem(check.Name + count, DriveFormsGrid);
                    driveRewards.Remove(check.Name);
                }
                else
                {
                    //check if user is currently in shop or puzzle and track item to Creations if so
                    if (CheckSynthPuzzle(pcsx2tracking))
                    {
                        TrackItem(check.Name + count, data.WorldsData["PuzzSynth"].worldGrid);
                    }
                    else
                    {
                        if (data.WorldsData.ContainsKey(world.previousworldName))
                        {
                            // add check to current world
                            TrackItem(check.Name + count, data.WorldsData[world.previousworldName].worldGrid);
                        }
                    }
                }
            }
        }

        private void UpdateCollectedItems()
        {
            foreach (ImportantCheck check in importantChecks)
            {
                // handle these separately due to the way they are stored in memory
                if (check.GetType() == typeof(Magic) || check.GetType() == typeof(TornPage))
                    continue;

                if (check.Obtained && collectedChecks.Contains(check) == false)
                {
                    // skip auto tracking final if it was forced and valor
                    if (check.Name == "Valor" && valor.genieFix == true)
                    {
                        valor.Obtained = false;
                    }
                    else if (check.Name == "Final")
                    {
                        // if forced Final, start tracking the Final Form check
                        if (!data.forcedFinal && stats.form == 5)
                        {
                            data.forcedFinal = true;
                            checkEveryCheck.TrackCheck(0x001D);
                        }
                        // if not forced Final, track Final Form check like normal
                        // else if Final was forced, check the tracked Final Form check
                        else if (!data.forcedFinal || checkEveryCheck.UpdateTargetMemory())
                        {
                            collectedChecks.Add(check);
                            newChecks.Add(check);
                        }
                    }
                    else
                    {
                        collectedChecks.Add(check);
                        newChecks.Add(check);
                    }
                }
            }
            TrackQuantities();
        }

        private void GetBoss(World world)
        {
            string boss = "None";

            //stops awarding points for a single boss each tick
            if (world.eventComplete == 1 && eventInProgress)
                return;
            else
                eventInProgress = false;

            //boss beaten events (taken mostly from progression code)

            switch (world.worldName)
            {
                case "SimulatedTwilightTown":
                    switch (world.roomNumber) //check based on room number now, then based on events in each room
                    {
                        case 34:
                            if (world.eventID1 == 157) // Twilight Thorn finish
                                boss = "Twilight Thorn";
                            break;
                        case 3:
                            if (world.eventID1 == 180) // Seifer Battle (Day 4)
                                boss = "Seifer";
                            break;
                        case 4:
                            //Tutorial Seifer shouldn't give points
                            //if (world.eventID1 == 77) // Tutorial 4 - Fighting
                            //    boss = "Seifer (1)";
                            if (world.eventID1 == 78) // Seifer I Battle
                                boss = "Seifer (2)";
                            break;
                        case 5:
                            if (world.eventID1 == 84) // Hayner Struggle
                                boss = "Hayner";
                            if (world.eventID1 == 85) // Vivi Struggle
                                boss = "Vivi";
                            if (world.eventID1 == 87) // Axel 1 Finish
                                boss = "Axel I";
                            if (world.eventID1 == 88) // Setzer Struggle
                                boss = "Setzer";
                            break;
                        case 20:
                            if (world.eventID1 == 137) // Axel 2 finish
                                boss = "Axel II";
                            break;
                        default:
                            return;
                    }
                    break;
                case "TwilightTown":
                    switch (world.roomNumber)
                    {
                        case 20:
                            if (world.eventID1 == 213) // Data Axel finish
                                boss = "Axel (Data)";
                            break;
                        case 4:
                            if (world.eventID1 == 181) // Seifer II Battle
                                boss = "Seifer (3)";
                            if (world.eventID1 == 182) // Hayner Battle (Struggle Competition)
                                boss = "Hayner (SR)";
                            if (world.eventID1 == 183) // Setzer Battle (Struggle Competition)
                                boss = "Setzer (SR)";
                            if (world.eventID1 == 184) // Seifer Battle (Struggle Competition)
                                boss = "Seifer (4)";
                            break;
                        default:
                            return;
                    }
                    break;
                case "HollowBastion":
                    switch (world.roomNumber)
                    {
                        case 4:
                            if (world.eventID1 == 55) // HB Demyx finish
                                boss = "Demyx";
                            else if (world.eventID1 == 114) // Data Demyx finish
                                boss = "Demyx (Data)";
                            break;
                        case 1:
                            if (world.eventID1 == 75) // Sephiroth finish
                                boss = "Sephiroth";
                            break;
                        default:
                            return;
                    }
                    break;
                case "BeastsCastle":
                    switch (world.roomNumber)
                    {
                        case 11:
                            if (world.eventID1 == 72) // Thresholder finish
                                boss = "Thresholder";
                            break;
                        case 3:
                            if (world.eventID1 == 69) // Beast finish
                                boss = "The Beast";
                            break;
                        case 5:
                            if (world.eventID1 == 78) // Shadow Stalker
                                boss = "Shadow Stalker";
                            if (world.eventID1 == 79) // Dark Thorn finish
                                boss = "Dark Thorn";
                            break;
                        case 15:
                            if (world.eventID1 == 82) // Xaldin finish
                                boss = "Xaldin";
                            else if (world.eventID1 == 97) // Data Xaldin finish
                                boss = "Xaldin (Data)";
                            break;
                        default:
                            return;
                    }
                    break;
                case "OlympusColiseum":
                    switch (world.roomNumber)
                    {
                        case 7:
                            if (world.eventID1 == 114) // Cerberus finish
                                boss = "Cerberus";
                            break;
                        case 8:
                            if (world.eventID1 == 116) // OC Pete finish
                                boss = "Pete OC II";
                            break;
                        case 18:
                            if (world.eventID1 == 171) // Hydra finish
                                boss = "Hydra";
                            break;
                        case 19:
                            if (world.eventID1 == 202) // Hades finish
                                boss = "Hades II (1)";
                            break;
                        case 34:
                            if (world.eventID1 == 151) // Zexion finish
                                boss = "Zexion";
                            else if (world.eventID1 == 152) // Data Zexion finish
                                boss = "Zexion (Data)";
                            break;
                        case 9: //Cups
                            if (world.eventID1 == 189 && world.cupRound == 10)
                                boss = "FF Team 1"; //Leon & Yuffie
                            if (world.eventID1 == 190 && world.cupRound == 10)
                                boss = "Cerberus (Cups)";
                            if (world.eventID1 == 191 && world.cupRound == 10)
                                boss = "Hercules";
                            if (world.eventID1 == 192 && world.cupRound == 10)
                                boss = "Hades Cups";
                            //paradox cups
                            if (world.eventID1 == 193 && world.cupRound == 10)
                                boss = "FF Team 2"; //Leon (3) & Yuffie (3)
                            if (world.eventID1 == 194 && world.cupRound == 10)
                                boss = "Cerberus (Cups)";
                            if (world.eventID1 == 195 && world.cupRound == 10)
                                boss = "Hercules";
                            //hades paradox
                            if (world.eventID1 == 196 && world.cupRound == 5)
                                boss = "Volcano Lord (Cups)";
                            if (world.eventID1 == 196 && world.cupRound == 10)
                                boss = "FF Team 3"; // Yuffie (1) & Tifa
                            if (world.eventID1 == 196 && world.cupRound == 15)
                                boss = "Blizzard Lord (Cups)";
                            if (world.eventID1 == 196 && world.cupRound == 20)
                                boss = "Pete Cups";
                            if (world.eventID1 == 196 && world.cupRound == 25)
                                boss = "FF Team 4"; // Cloud & Tifa (1)
                            if (world.eventID1 == 196 && world.cupRound == 30)
                                boss = "Hades Cups";
                            if (world.eventID1 == 196 && world.cupRound == 40)
                                boss = "FF Team 5"; // Leon (1) & Cloud (1)
                            if (world.eventID1 == 196 && world.cupRound == 48)
                                boss = "Cerberus (Cups)";
                            if (world.eventID1 == 196 && world.cupRound == 49)
                                boss = "FF Team 6"; // Leon (2), Cloud (2), Yuffie (2), & Tifa (2)
                            if (world.eventID1 == 196 && world.cupRound == 50)
                                boss = "Hades II";
                            break;
                        default:
                            return;
                    }
                    break;
                case "Agrabah":
                    switch (world.roomNumber)
                    {
                        case 3:
                            if (world.eventID1 == 59) // Lords finish
                                boss = "Twin Lords";
                            break;
                        case 5:
                            if (world.eventID1 == 62) // Genie Jafar finish
                                boss = "Jafar";
                            break;
                        case 33:
                            if (world.eventID1 == 142) // Lexaeus finish
                                boss = "Lexaeus";
                            else if (world.eventID1 == 147) // Data Lexaeus finish
                                boss = "Lexaeus (Data)";
                            break;
                        default:
                            return;
                    }
                    break;
                case "LandofDragons":
                    switch (world.roomNumber)
                    {
                        case 9:
                            if (world.eventID1 == 75) // Shan Yu finish
                                boss = "Shan-Yu";
                            break;
                        case 7:
                            if (world.eventID1 == 76) // Riku
                                boss = "Riku";
                            break;
                        case 8:
                            if (world.eventID1 == 79) // Storm Rider finish
                                boss = "Storm Rider";
                            break;
                        default:
                            return;
                    }
                    break;
                case "PrideLands":
                    switch (world.roomNumber)
                    {
                        case 14:
                            if (world.eventID1 == 55) // Scar finish
                                boss = "Scar";
                            break;
                        case 15:
                            if (world.eventID1 == 59) // Groundshaker finish
                                boss = "Groundshaker";
                            break;
                        default:
                            return;
                    }
                    break;
                case "DisneyCastle":
                    switch (world.roomNumber)
                    {
                        case 1:
                            if (world.eventID1 == 58) // Old Pete finish
                                boss = "Past Pete";
                            break;
                        case 2:
                            if (world.eventID1 == 52) // Boat Pete finish
                                boss = "Boat Pete";
                            break;
                        case 3:
                            if (world.eventID1 == 53) // DC Pete finish
                                boss = "Pete TR";
                            break;
                        case 38:
                            if (world.eventID1 == 145) // Marluxia finish
                                boss = "Marluxia";
                            else if (world.eventID1 == 150) // Data Marluxia finish
                                boss = "Marluxia (Data)";
                            break;
                        case 7:
                            if (world.eventID1 == 67) // Lingering Will finish
                                boss = "Terra";
                            break;
                        default:
                            return;
                    }
                    break;
                case "HalloweenTown":
                    switch (world.roomNumber)
                    {
                        case 3:
                            if (world.eventID1 == 52) // Prison Keeper finish
                                boss = "Prison Keeper";
                            break;
                        case 9:
                            if (world.eventID1 == 55) // Oogie Boogie finish
                                boss = "Oogie Boogie";
                            break;
                        case 7:
                            if (world.eventID1 == 64) // Experiment finish
                                boss = "The Experiment";
                            break;
                        case 32:
                            if (world.eventID1 == 115) // Vexen finish
                                boss = "Vexen";
                            if (world.eventID1 == 146) // Data Vexen finish
                                boss = "Vexen (Data)";
                            break;
                        default:
                            return;
                    }
                    break;
                case "PortRoyal":
                    switch (world.roomNumber)
                    {
                        case 10:
                            if (world.eventID1 == 60) // Barbossa finish
                                boss = "Barbossa";
                            break;
                        case 18:
                            if (world.eventID1 == 85) // Grim Reaper 1 finish
                                boss = "Grim Reaper I";
                            break;
                        case 1:
                            if (world.eventID1 == 54) // Grim Reaper 2 finish
                                boss = "Grim Reaper II";
                            break;
                        default:
                            return;
                    }
                    break;
                case "SpaceParanoids":
                    switch (world.roomNumber)
                    {
                        case 4:
                            if (world.eventID1 == 55) // Hostile Program finish
                                boss = "Hostile Program";
                            break;
                        case 9:
                            if (world.eventID1 == 58) // Sark finish
                                boss = "Sark";
                            else if (world.eventID1 == 59) // MCP finish
                                boss = "MCP";
                            break;
                        case 33:
                            if (world.eventID1 == 143) // Larxene finish
                                boss = "Larxene";
                            else if (world.eventID1 == 148) // Data Larxene finish
                                boss = "Larxene (Data)";
                            break;
                        default:
                            return;
                    }
                    break;
                case "TWTNW":
                    switch (world.roomNumber)
                    {
                        case 21:
                            if (world.eventID1 == 65) // Roxas finish
                                boss = "Roxas";
                            else if (world.eventID1 == 99) // Data Roxas finish
                                boss = "Roxas (Data)";
                            break;
                        case 10:
                            if (world.eventID1 == 57) // Xigbar finish
                                boss = "Xigbar";
                            else if (world.eventID1 == 100) // Data Xigbar finish
                                boss = "Xigbar (Data)";
                            break;
                        case 14:
                            if (world.eventID1 == 58) // Luxord finish
                                boss = "Luxord";
                            else if (world.eventID1 == 101) // Data Luxord finish
                                boss = "Luxord (Data)";
                            break;
                        case 15:
                            if (world.eventID1 == 56) // Saix finish
                                boss = "Saix";
                            else if (world.eventID1 == 102) // Data Saix finish
                                boss = "Saix (Data)";
                            break;
                        case 19:
                            if (world.eventID1 == 59) // Xemnas 1 finish
                                boss = "Xemnas";
                            else if (world.eventID1 == 97) // Data Xemnas I finish
                                boss = "Xemnas (Data)";
                            break;
                        case 20:
                            if (world.eventID1 == 74) // Final Xemnas finish
                                boss = "Final Xemnas";
                            else if (world.eventID1 == 98) // Data Final Xemnas finish
                                boss = "Final Xemnas (Data)";
                            break;
                        case 23:
                            if (world.eventID1 == 73) // Armor Xemnas II
                                boss = "Armor Xemnas II";
                            break;
                        case 24:
                            if (world.eventID1 == 71) // Armor Xemnas I
                                boss = "Armor Xemnas I";
                            break;
                        default:
                            return;
                    }
                    break;
                default: //return if any other world
                    return;
            }

            if (world.eventComplete != 1 || boss == "None")
                return;

            //if the boss was found and beaten then set flag
            //we do this to stop things happening every frame
            if (world.eventComplete == 1)
                eventInProgress = true;

            App.logger?.Record("Beaten Boss: " + boss);

            //get points for boss kills
            GetBossPoints(boss);

        }

        private void GetBossPoints(string boss)
        {
            int points;
            string bossType;
            string replacementType;

            if (boss == "Twin Lords")
            {
                if (data.BossRandoFound)
                {
                    //BlizzardLord
                    replacementType = Codes.FindBossType(data.BossList["Blizzard Lord"]);
                    if (replacementType == "Unknown")
                    {
                        Console.WriteLine("Unknown Replacement Boss: " + data.BossList["Blizzard Lord"] + ". Using default points.");

                        if (App.logger != null)
                            App.logger.Record("Unknown Replacement Boss: " + data.BossList["Blizzard Lord"] + ". Using default points.");

                        replacementType = "boss_other";
                    }
                    else
                    {
                        if (App.logger != null)
                            App.logger.Record("Blizzard Lord Replacement: " + data.BossList["Blizzard Lord"]);
                    }

                    points = data.PointsDatanew[replacementType];

                    //Volcano Lord
                    replacementType = Codes.FindBossType(data.BossList["Volcano Lord"]);
                    if (replacementType == "Unknown")
                    {
                        Console.WriteLine("Unknown Replacement Boss: " + data.BossList["Volcano Lord"] + ". Using default points.");

                        if (App.logger != null)
                            App.logger.Record("Unknown Replacement Boss: " + data.BossList["Volcano Lord"] + ". Using default points.");

                        replacementType = "boss_other";
                    }
                    else
                    {
                        if (App.logger != null)
                            App.logger.Record("Volcano Lord Replacement: " + data.BossList["Volcano Lord"]);
                    }

                    points += data.PointsDatanew[replacementType];

                    //bonus points here should be sum of both boss types / 2
                    if (points > 1)
                        points += points / 2;
                }
                else
                {
                    points = data.PointsDatanew["boss_other"] * 2;
                }
            }
            else if (boss.StartsWith("FF Team"))
            {
                string[] test = { "Unknown", "Unknown", "Unknown", "Unknown" };

                if (boss == "FF Team 6")
                {
                    test[0] = "Leon (2)";
                    test[1] = "Cloud (2)";
                    test[2] = "Yuffie (2)";
                    test[3] = "Tifa (2)";

                    replacementType = Codes.FindBossType(data.BossList[test[0]]);
                    if (replacementType == "Unknown")
                    {
                        //Console.WriteLine("Unknown Replacement Boss: " + data.BossList[test[0]] + ". Using default points.");
                        App.logger?.Record("Unknown Replacement Boss: " + data.BossList[test[0]] + ". Using default points.");
                        replacementType = "boss_other";
                    }
                    else App.logger?.Record(test[0] + " Replacement: " + data.BossList[test[0]]);

                    points = data.PointsDatanew[replacementType];

                    replacementType = Codes.FindBossType(data.BossList[test[1]]);
                    if (replacementType == "Unknown")
                    {
                        //Console.WriteLine("Unknown Replacement Boss: " + data.BossList[test[1]] + ". Using default points.");
                        App.logger?.Record("Unknown Replacement Boss: " + data.BossList[test[1]] + ". Using default points.");
                        replacementType = "boss_other";
                    }
                    else App.logger?.Record(test[1] + " Replacement: " + data.BossList[test[1]]);

                    points += data.PointsDatanew[replacementType];

                    replacementType = Codes.FindBossType(data.BossList[test[2]]);
                    if (replacementType == "Unknown")
                    {
                        //Console.WriteLine("Unknown Replacement Boss: " + data.BossList[test[2]] + ". Using default points.");
                        App.logger?.Record("Unknown Replacement Boss: " + data.BossList[test[2]] + ". Using default points.");
                        replacementType = "boss_other";
                    }
                    else App.logger?.Record(test[2] + " Replacement: " + data.BossList[test[2]]);

                    points += data.PointsDatanew[replacementType];

                    replacementType = Codes.FindBossType(data.BossList[test[3]]);
                    if (replacementType == "Unknown")
                    {
                        //Console.WriteLine("Unknown Replacement Boss: " + data.BossList[test[3]] + ". Using default points.");
                        App.logger?.Record("Unknown Replacement Boss: " + data.BossList[test[3]] + ". Using default points.");
                        replacementType = "boss_other";
                    }
                    else App.logger?.Record(test[3] + " Replacement: " + data.BossList[test[3]]);

                    points += data.PointsDatanew[replacementType];

                    //bonus points here should be sum of both boss types / 2
                    if (points > 1)
                        points += points / 2;
                }
                else
                {
                    if (boss == "FF Team 1")
                    {
                        test[0] = "Leon";
                        test[1] = "Yuffie";
                    }
                    if (boss == "FF Team 2")
                    {
                        test[0] = "Leon (3)";
                        test[1] = "Yuffie (3)";
                    }
                    if (boss == "FF Team 3")
                    {
                        test[0] = "Yuffie (1)";
                        test[1] = "Tifa";
                    }
                    if (boss == "FF Team 4")
                    {
                        test[0] = "Cloud";
                        test[1] = "Tifa (1)";
                    }
                    if (boss == "FF Team 5")
                    {
                        test[0] = "Leon (1)";
                        test[1] = "Cloud (1)";
                    }

                    replacementType = Codes.FindBossType(data.BossList[test[0]]);
                    if (replacementType == "Unknown")
                    {
                        //Console.WriteLine("Unknown Replacement Boss: " + data.BossList[test[0]] + ". Using default points.");
                        App.logger?.Record("Unknown Replacement Boss: " + data.BossList[test[0]] + ". Using default points.");
                        replacementType = "boss_other";
                    }
                    else App.logger?.Record(test[0] + " Replacement: " + data.BossList[test[0]]);

                    points = data.PointsDatanew[replacementType];

                    replacementType = Codes.FindBossType(data.BossList[test[1]]);
                    if (replacementType == "Unknown")
                    {
                        //Console.WriteLine("Unknown Replacement Boss: " + data.BossList[test[1]] + ". Using default points.");
                        App.logger?.Record("Unknown Replacement Boss: " + data.BossList[test[1]] + ". Using default points.");
                        replacementType = "boss_other";
                    }
                    else App.logger?.Record(test[1] + " Replacement: " + data.BossList[test[1]]);

                    points += data.PointsDatanew[replacementType];

                    //bonus points here should be sum of both boss types / 2
                    if (points > 1)
                        points += points / 2;
                }

            }
            else
            {
                bossType = Codes.FindBossType(boss);
                if (bossType == "Unknown")
                {
                    Console.WriteLine("Unknown Boss: " + boss + ". Using default points.");

                    if (App.logger != null)
                        App.logger.Record("Unknown Boss: " + boss + ". Using default points.");

                    bossType = "boss_other";
                }

                if (data.BossRandoFound && data.BossList.ContainsKey(boss))
                {
                    replacementType = Codes.FindBossType(data.BossList[boss]);

                    if (replacementType == "Unknown")
                    {
                        Console.WriteLine("Unknown Replacement Boss: " + data.BossList[boss] + ". Using default points.");

                        if (App.logger != null)
                            App.logger.Record("Unknown Replacement Boss: " + data.BossList[boss] + ". Using default points.");

                        replacementType = "boss_other";
                    }
                    else
                    {
                        if (App.logger != null)
                            App.logger.Record(boss + " Replacement: " + data.BossList[boss]);
                    }

                    points = data.PointsDatanew[replacementType];

                    //add extra points for bosses in special arenas
                    int bonuspoints = 0;
                    switch (bossType)
                    {
                        case "boss_as":
                        case "boss_datas":
                        case "boss_sephi":
                        case "boss_terra":
                        case "boss_final":
                            bonuspoints += data.PointsDatanew[bossType];
                            break;
                    }

                    points += bonuspoints;
                }
                else
                {
                    points = data.PointsDatanew[bossType];

                    //temp fix. might change if final xemnas gets randomized
                    //for now this just makes worth data points * 2
                    //if (boss == "Final Xemnas (Data)")
                    //    points += data.PointsDatanew["boss_datas"];

                    //logging
                    if(data.BossRandoFound)
                    {
                        if (App.logger != null)
                            App.logger.Record("No replacement found? Boss: " + boss);
                    }
                }
            }

            UpdatePointScore(points);
        }

        private void HighlightWorld(World world)
        {
            if (WorldHighlightOption.IsChecked == false)
                return;

            if (world.previousworldName != null && data.WorldsData.ContainsKey(world.previousworldName))
            {
                foreach (Rectangle Box in data.WorldsData[world.previousworldName].top.Children.OfType<Rectangle>().Where(Box => Box.Name.EndsWith("SelWG")))
                {
                    Box.Visibility = Visibility.Collapsed;
                }
            }

            if (data.WorldsData.ContainsKey(world.worldName))
            {
                foreach (Rectangle Box in data.WorldsData[world.worldName].top.Children.OfType<Rectangle>().Where(Box => Box.Name.EndsWith("SelWG")))
                {
                    Box.Visibility = Visibility.Visible;
                }
            }
        }

        ///
        /// Bindings & helpers
        ///

        private void SetBindings()
        {
            BindWeapon(SorasHeartWeapon, "Weapon", stats);

            //changes opacity for stat icons
            BindAbility(HighJump, "Obtained", highJump);
            BindAbility(QuickRun, "Obtained", quickRun);
            BindAbility(DodgeRoll, "Obtained", dodgeRoll);
            BindAbility(AerialDodge, "Obtained", aerialDodge);
            BindAbility(Glide, "Obtained", glide);

            BindForm(ValorM, "Obtained", valor);
            BindForm(WisdomM, "Obtained", wisdom);
            BindForm(LimitM, "Obtained", limit);
            BindForm(MasterM, "Obtained", master);
            BindForm(FinalM, "Obtained", final);
        }

        private void BindForm(ContentControl img, string property, object source)
        {
            Binding binding = new Binding(property);
            binding.Source = source;
            binding.Converter = new ObtainedConverter();
            img.SetBinding(OpacityProperty, binding);
        }

        private void BindAbility(ContentControl img, string property, object source)
        {
            Binding binding = new Binding(property);
            binding.Source = source;
            binding.Converter = new ObtainedConverter();
            img.SetBinding(OpacityProperty, binding);
        }

        private void BindWeapon(Image img, string property, object source)
        {
            Binding binding = new Binding(property);
            binding.Source = source;
            binding.Converter = new WeaponConverter();
            img.SetBinding(Image.SourceProperty, binding);
        }

        private string BytesToHex(byte[] bytes)
        {
            if (Enumerable.SequenceEqual(bytes, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }))
            {
                return "Service not started. Waiting for PCSX2";
            }
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public string GetWorld()
        {
            return world.worldName;
        }

        public void UpdateUsedPages()
        {
            data.usedPages++;
        }

        public int GetUsedPages()
        {
            return data.usedPages;
        }

        public void UpdateFormProgression()
        {
            int found = 0;
            string drives = "";
            bool OldToggled = Properties.Settings.Default.OldProg;
            bool CustomToggled = Properties.Settings.Default.CustomIcons;
            string Prog = "Min-"; //Default
            if (OldToggled)
                Prog = "Old-";
            if (CustomProgFound && CustomToggled)
                Prog = "Cus-";

            if (ValorM.Opacity == 1)
                found++;
            if (WisdomM.Opacity == 1)
                found++;
            if (LimitM.Opacity == 1)
                found++;
            if (MasterM.Opacity == 1)
                found++;
            if (FinalM.Opacity == 1)
                found++;


            switch (found)
            {
                case 1:
                    drives = "Drive3";
                    break;
                case 2:
                    drives = "Drive4";
                    break;
                case 3:
                    drives = "Drive5";
                    break;
                case 4:
                    drives = "Drive6";
                    break;
                case 5:
                    drives = "Drive7";
                    break;
                default:
                    drives = "Drive2";
                    break;
            }

            DriveFormsCap.SetResourceReference(ContentProperty, Prog + drives);
        }

        //progression hints - compare last saved progression point
        //must be checked this way cause of OnTimedEvent
        public void UpdateProgressionPoints(string worldName, int prog)
        {
            //if event is current, skip
            if ((world.eventID1 == data.PrevEventID1 && world.eventID3 == data.PrevEventID3
                && world.worldName == data.PrevWorld && world.roomNumber == data.PrevRoomNum)
                || !data.UsingProgressionHints)
                return;

            AddProgressionPoints(GetProgressionPointsReward(worldName, prog));
            data.PrevEventID1 = world.eventID1;
            data.PrevEventID3 = world.eventID3;
            data.PrevWorld = world.worldName;
            data.PrevRoomNum = world.roomNumber;
        }
        public void UpdateProgressionPointsTWTNW(string worldName)
        {
            //if event is current, skip
            if ((world.eventID1 == data.PrevEventID1 && world.eventID3 == data.PrevEventID3
                && world.worldName == data.PrevWorld && world.roomNumber == data.PrevRoomNum)
                || !data.UsingProgressionHints)
                return;
            Console.WriteLine("Defeated Final Xemnas");
            data.TWTNW_ProgressionValues.Add(200);
            AddProgressionPoints(GetProgressionPointsReward(worldName, data.TWTNW_ProgressionValues.Count));
            data.TWTNW_ProgressionValues.RemoveAt(data.TWTNW_ProgressionValues.Count - 1);
            data.TWTNW_ProgressionValues.Add(-200);
            AddProgressionPoints(GetProgressionPointsReward(worldName, data.TWTNW_ProgressionValues.Count));
            data.TWTNW_ProgressionValues.RemoveAt(data.TWTNW_ProgressionValues.Count - 1);
            data.PrevEventID1 = world.eventID1;
            data.PrevEventID3 = world.eventID3;
            data.PrevWorld = world.worldName;
            data.PrevRoomNum = world.roomNumber;
        }
    }
}