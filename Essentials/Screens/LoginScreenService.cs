using DataBaseFormats.Entities.Players;
using MUD_Server.DevTools;
using MUD_Server.Entry;
using MUD_Server.Essentials.Commands;
using MUD_Server.Essentials.Commands.CommandNodeCollections;
using MUD_Server.Essentials.Framework;
using MUD_Server.Essentials.Storing;
using MUD_Server.Game.Entities.Players;
using MUD_Server.Game.Players.Entities.Extensions;
using MUD_Server.Network;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MUD_Server.Essentials.Screens
{
    public class LoginScreenService : CommandNodeCollection
    {
        private UserSQL UserDB { get; }

        public LoginScreenService(UserSQL userDB, Server server)
        {
            UserDB = userDB;

            Entry = new CommandNodeBuilder(this, new Description(
                $"{Color.Parse("white:intense")}Welcome to {Color.Parse("cyan:bold")}Golmodoth Online{Color.Parse("white:intense")} {Color.Parse("white")}{Startup.Version}{Color.Parse("white")}\n\n" +
                $"{Color.Parse("white")}Proceed to {Color.Parse("white:intense")}Login {Color.Parse("white")}or {Color.Parse("white:intense")}Sign Up{Color.Parse("white")}.\n\n"))
                .WithOptionalExecution(x => { x.Cache.Clear(); }).Build();

            var Login = new CommandNodeBuilder(this, Entry, "(?i)login", new Description($"\nEnter {Color.Parse("white:intense")}Username: ")).Build();
            CreateConnection(Entry, Login); InitSignUp(Entry);

            var Login_Username = new CommandNodeBuilder(this, Entry, @"\w+", new Description($"\nEnter {Color.Parse("white:intense")}Password: "))
                .WithInput(0)
                .Build();

            //Last pass 
            var Login_Password = new CommandNodeBuilder(this, Entry, @"\w+", 
                e =>
                {
                    e.CinematicsUsed.Clear();

                    return Task.FromResult(
                        $"Logged in as [ {Color.Parse("white:bold")}{e.Username} {Color.Parse("white")}]\n\n" +
                        string.Format("{0}\n\n", e.Characters.Count() > 0 ? $"{Color.Parse("green")}Enter Game {Color.Parse("white")}[ {Color.Parse("white:intense")}Character_Name{Color.Parse("white")} ]" : $"{Color.Parse("yellow")}Create Character"));
                })
                .WithInput(0)
                .DisableBackwards()
                .WithOptionalExecution(async e =>
                {
                    if (await UserDB.GetUser(e, e.Cache[0], e.Cache[1]) == null)
                    {
                        e.Cache.Clear();
                        await e.SetActiveCommandNode(Entry, clearScreen: true, reason: $"Profile either does not exist or {Color.Parse("white:intense")}Username{Color.Parse("white")}/{Color.Parse("white:intense")}Password {Color.Parse("white")}was misspelled.\n");
                    }
                    else if (e.Username == "LtLi0n")
                    {
                        e.Cache.Clear();
                        e.Characters = e.Characters.Append(
                            new Character()
                            {
                                Name = "LtLi0n",
                                Gender = Gender.Male,
                                Class = Class.Warrior,
                                Zodiac = Zodiac.Leo,
                                Race = Race.Human,
                                IsNew = true
                            });
                    }
                }).Build();

            //To log out
            var Login_Logout = new CommandNodeBuilder(this, Login_Password, "(?i)logout", Description.Empty)
                .WithOptionalExecution(async e =>
                {
                    e.Cache.Clear();
                    e.CinematicsUsed.Clear();
                    await e.SetActiveCommandNode(Entry);
                }).Build();

            var enterWorldNode = new CommandNodeBuilder(this, Login_Password, @"(?i)enter game (\w{3,15})", new Description("Welcome back"))
                .Destination()
                .WithInput(1)
                .WithOptionalExecution(async e =>
                {
                    if (e.Characters.Where(c => c.Name == e.Cache[0]).Count() == 1)
                    {
                        e.OnlineCharacter = new SocketCharacter(e, e.Characters.Where(c => c.Name == e.Cache[0]).First());
                        e.Cache.Clear();

                        //If character is fresh, show introduction screen
                        if (e.OnlineCharacter.IsNew) await e.SetActiveCommandNode(CommandNodeServiceCollection.IntroductionScreen.Entry, false);
                        else await e.SetActiveCommandNode(null, false);
                    }
                    else await e.SetActiveCommandNode(Login_Password, true, true, "Couldn't find this character.\n");
                }).Build();

            CreateConnections(Login, Login_Username); CreateConnections(Login_Username, Login_Password);

            CreateConnections(Login_Password, enterWorldNode, Login_Logout);

            InitCharacterCreation(Login_Password); DevConsoleService.Bind(Login_Password, this, UserDB, server);
        }

        private void InitSignUp(CommandNode parent)
        {
            CommandNode refNode;

            //Sign Up Entry
            {
                var signUpEntry = new CommandNodeBuilder(this, parent, "(?i)sign up", new Description($"Enter {Color.Parse("white:intense")}Email{Color.Parse("white")}: "))
                    .WithRegexExplanation("Email was not recognized as a supported format.")
                    .WithOptionalExecution(e => e.Cache.Clear()).Build();

                CreateConnection(parent, signUpEntry);
                refNode = signUpEntry;
            }

            //Email Register
            {
                var emailRegister = new CommandNodeBuilder(this, parent, @"[A-Z0-9a-z._%+-]{2,30}@[A-Za-z0-9.-]+\.[A-Za-z]{2,20}", new Description($"Repeat the {Color.Parse("white:intense")}selected Email{Color.Parse("white")}: "))
                    .WithInput(0)
                    .WithOptionalExecution(async e =>
                    {
                        if (await UserDB.UserExistsWithEmail(e.Cache[0]))
                        {
                            e.Cache.Clear();
                            await e.SetActiveCommandNode(parent, true, true, "This email is already registered.\n");
                        }
                        else if (e.RecentInput != e.Cache[0])
                        {
                            e.Cache.Clear();
                            await e.SetActiveCommandNode(parent, true, true, $"{Color.Parse("white:intense")}Emails did not match [regex != input].\n");
                        }
                    }).Build();

                CreateConnection(refNode, emailRegister);
                refNode = emailRegister;
            }
            //Email Confirm
            {
                var emailConfirm = new CommandNodeBuilder(this, parent, @".+", new Description($"Select {Color.Parse("white:intense")}Username{Color.Parse("white")}: ")).WithRegexExplanation("Usernames cannot contain any special characters or spaces.")
                    .WithOptionalExecution(async e =>
                    {
                        if (e.RecentInput != e.Cache[0])
                        {
                            e.Cache.Clear();
                            await e.SetActiveCommandNode(parent, true, true, $"{Color.Parse("white:intense")}Emails did not match.\n");
                        }
                    }).Build();

                CreateConnection(refNode, emailConfirm);
                refNode = emailConfirm;
            }
            //Login Register
            {
                var loginRegister = new CommandNodeBuilder(this, parent, @"[A-Z|a-z|0-9]{4,30}", new Description($"Select {Color.Parse("white:intense")}Password{Color.Parse("white")}: "))
                .WithRegexExplanation("Password must be at least 6 characters but not less than 30.")
                .WithInput(0)
                .WithOptionalExecution(async e =>
                {
                    if (e.RecentInput != e.Cache[1])
                    {
                        e.Cache.Clear();
                        await e.SetActiveCommandNode(parent, true, true,
                            $"{Color.Parse("white:underline")}Username was not recognized as a supported format.{Color.Parse("white:intense")}\n" +
                            "Possible triggers:\n" +
                            " Username was shorter than 4 or longer than 30 characters\n" +
                            " Username contained odd characters or a space. (Supported - Upper/lower case + numbers)\n");
                    }
                    else if (await UserDB.UserExistsWithUsername(e.Cache[1]))
                    {
                        e.Cache.Clear();
                        await e.SetActiveCommandNode(parent, true, true, $"{Color.Parse("white:intense")}User with this Username already exists.\n");
                    }
                }).Build();

                CreateConnection(refNode, loginRegister);
                refNode = loginRegister;
            }
            //Password Register
            {
                var passwordRegister = new CommandNodeBuilder(this, parent, @".{6,30}", new Description($"Repeat the {Color.Parse("white:intense")}selected Password{Color.Parse("white")}: "))
                    .WithInput(0)
                    .Build();

                CreateConnection(refNode, passwordRegister);
                refNode = passwordRegister;
            }
            //Password Confirm
            {
                var passwordConfirm = new CommandNodeBuilder(this, parent, @".+", Description.Empty)
                .Destination()
                .WithOptionalExecution(async e =>
                {
                    if (e.Cache[2] != e.RecentInput) await e.SetActiveCommandNode(parent, true, true, $"{Color.Parse("white:intense")}Passwords did not match.\n");
                    else
                    {
                        await UserDB.CreateUser(e.Cache[0], e.Cache[1], e.Cache[2]);
                        e.Cache.Clear();
                        await e.SetActiveCommandNode(parent, true, true, $"{Color.Parse("green:intense")}Account created! Feel free to login.\n");
                    }
                }).Build();

                CreateConnection(refNode, passwordConfirm);
            }
        }

        private void InitCharacterCreation(CommandNode parent)
        {
            CommandNode refNode;

            //Character Creation
            {
                var createCharacter = new CommandNodeBuilder(this, parent, "(?i)create character",
                    e =>
                    {
                        string racesInfo = string.Empty;
                        foreach (Race r in Enum.GetValues(typeof(Race))) racesInfo += $"{Color.Parse("green")}{Enum.GetName(typeof(Race), r)}{Color.Parse("white")} - {r.GetDescription()}\n\n";

                        return Task.FromResult(
                            $"[ {Color.Parse("yellow:intense")}Select Race {Color.Parse("white")}]\n\n" +
                            $"{racesInfo.Remove(racesInfo.Length - 1, 1)}\n");
                    }).WithOptionalExecution(e => Task.Run(() => e.Cache.Clear())).Build();

                CreateConnection(parent, createCharacter);
                refNode = createCharacter;
            }

            //Race Selection
            {
                var selectRace = new CommandNodeBuilder(this, refNode, GetSelectionRegex<Race>(),
                    e =>
                    {
                        string classesInfo = string.Empty;
                        foreach (Class r in Enum.GetValues(typeof(Class))) classesInfo += $"{Color.Parse("green")}{Enum.GetName(typeof(Class), r)}{Color.Parse("white")} - {r.GetDescription()}\n\n";

                        return Task.FromResult(
                            $"[ {Color.Parse("yellow:intense")}Select Class {Color.Parse("white")}]\n\n" +
                            $"{classesInfo.Remove(classesInfo.Length - 1, 1)}\n");
                    }).WithInput(1).Build();

                CreateConnection(refNode, selectRace);
                refNode = selectRace;
            }

            //Class Selection
            {
                var selectClass = new CommandNodeBuilder(this, refNode, GetSelectionRegex<Class>(),
                    e =>
                    {
                        string zodiacsInfo = string.Empty;

                        foreach (Zodiac z in Enum.GetValues(typeof(Zodiac))) zodiacsInfo += $"{Color.Parse("green")}{Enum.GetName(typeof(Zodiac), z)}{Color.Parse("white")} - {z.GetDescription()}\n";

                        return Task.FromResult(
                            $"[ {Color.Parse("yellow:intense")}Select Zodiac {Color.Parse("white")}]\n\n" +
                            $" Note:Zodiacs only change how you perceive your character.\n\n" +
                            $"{Color.Parse("white:underline")}This choice won't influence your access to zones or interactions with NPCs{Color.Parse("white")}.\n\n" +
                            $"{zodiacsInfo}\n");
                    }).WithInput(1).Build();

                CreateConnection(refNode, selectClass);
                refNode = selectClass;
            }

            //Zodiac Selection
            {
                var selectZodiac = new CommandNodeBuilder(this, refNode, GetSelectionRegex<Zodiac>(), 
                    e =>
                    {
                        return Task.FromResult($"[ {Color.Parse("yellow:intense")}Select Gender {Color.Parse("white")}] - {Color.Parse("white:underline")}Male{Color.Parse("white")} / {Color.Parse("white:underline")}Female{Color.Parse("white")}\n\n");
                    }).WithInput(1).Build();

                CreateConnection(refNode, selectZodiac);
                refNode = selectZodiac;
            }

            //Gender Selection
            {
                var selectGender = new CommandNodeBuilder(this, refNode, GetSelectionRegex<Gender>(), $"[ {Color.Parse("yellow:intense")}Select Name {Color.Parse("white")}]: ")
                .WithRegexExplanation($"{Color.Parse("red")}Selected name can not be less than 3 or more than 20 characters in length.")
                .WithInput(1).Build();

                CreateConnection(refNode, selectGender);
                refNode = selectGender;
            }

            //Name Selection
            {
                var selectName = new CommandNodeBuilder(this, refNode, @"\w{3,15}",
                    e =>
                    {
                        Gender gender = (Gender)Enum.Parse(typeof(Gender), e.Cache[3], true);

                        return Task.FromResult(
                            $"{Color.Parse("white")}Are you sure you want to create character - {Color.Parse("white:bold")}{e.Cache.Last()} {Color.Parse(gender == Gender.Male ? "cyan" : "purple")}{Enum.GetName(typeof(Gender), gender)}{Color.Parse("white")} {Color.QuestionString}\n\n" +
                            $"{Color.Parse("green")}Race: {Color.Parse("white")}{Enum.GetName(typeof(Race), Enum.Parse(typeof(Race), e.Cache[0], true))}\n" +
                            $"{Color.Parse("green")}Class: {Color.Parse("white")}{Enum.GetName(typeof(Class), Enum.Parse(typeof(Class), e.Cache[1], true))}\n" +
                            $"{Color.Parse("green")}Zodiac: {Color.Parse("white")}{Enum.GetName(typeof(Zodiac), Enum.Parse(typeof(Zodiac), e.Cache[2], true))}\n");
                    }).WithInput(0).Build();

                CreateConnection(refNode, selectName);
                refNode = selectName;
            }

            //Create Character Confirm
            {
                var confirm = new CommandNodeBuilder(this, refNode, @"(?i)y|n", Description.Empty)
                    .WithInput(0)
                    .WithOptionalExecution(async e =>
                    {
                        char response = e.Cache.Last().ToLower()[0];

                        string notify = Color.Parse("white:underline").ToString();

                        if (response == 'y')
                        {
                            if (e.Characters.Where(x => x.Name == e.Cache[3]).Count() > 0)
                            {
                                notify += "You, Sir, cannot create clones yet.";
                            }
                            else
                            {
                                var character = new Character()
                                {
                                    Name = e.Cache[4],
                                    Race = (Race)Enum.Parse(typeof(Race), e.Cache[0], true),
                                    Class = (Class)Enum.Parse(typeof(Class), e.Cache[1], true),
                                    Zodiac = (Zodiac)Enum.Parse(typeof(Zodiac), e.Cache[2], true),
                                    Gender = (Gender)Enum.Parse(typeof(Gender), e.Cache[3], true),
                                };

                                character.Init();

                                e.Characters = e.Characters.Append(character);

                                notify += $"{character.Name} Created!";
                            }
                        }
                        else if (response == 'n') notify += "Character creation successfully cancelled.";

                        notify += '\n';

                        e.Cache.Clear();
                        await e.SetActiveCommandNode(parent, true, true, notify);
                    }).Build();

                CreateConnection(refNode, confirm);
            }
        }
        private string GetSelectionRegex<T>()
        {
            string selectCharEnumRegex = $"(?i)(";

            foreach (T z in Enum.GetValues(typeof(T))) selectCharEnumRegex += Enum.GetName(typeof(T), z) + '|';
            return selectCharEnumRegex.Remove(selectCharEnumRegex.Length - 1) + ')';
        }
    }
}
