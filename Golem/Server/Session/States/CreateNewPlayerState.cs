using Golem.Game.Mobiles;
using Golem.Server.Extensions;
using Golem.Server.UserInterface;

namespace Golem.Server.Session.States
{
    public class CreateNewPlayerState : SessionState
    {
        public enum State
        {
            EnterForename,
            EnterPassword,
            EnterShortDescription,
            EnterDescription,
            SelectGender,
            SelectPronouns,
            Finished
        }

        private State _currentState;

        private string _forename;
        private string _password;
        private string _shortDescription;
        private string _description;
        private PlayerGender _gender;
        private PlayerPronouns _pronouns;

        private TextEditor _textEditor;


        public CreateNewPlayerState(string playerName)
        {
            _forename = playerName;

            _textEditor = new TextEditor
            {
                Description = "Please enter your character description: \r\n" +
                              "This is a long multiline description of what your character looks like.\r\n" +
                              "Try to be as descriptive as possible while keeping the description (after \r\n" +
                              "formatting) to a page of text (23 lines).\r\n" +
                              "You should not use you characters name in the description, as someone can't\r\n" +
                              "infer that from just looking at you.\r\n" +
                              "Descriptions of clothing and gear should not be in here, as what you are\r\n" +
                              "wearing will be determined by the equipment system in game.\r\n" +
                              "You should avoid subjective descriptions and it should be written in the third \r\n" +
                              "person while avoiding addressing the reader directly with words like \"you\".\r\n\rn" +
                              "I suggest using your operating systems text editor to write your description\r\n" +
                              "so you can edit easily and make sure it is correct before pasting the final\r\n" +
                              "product here and formatting it by typing the single letter command (f)\r\n\r\n" +
                              "Before saving (s) make sure you have formatted your text (f)."
            };
        }

        public void ChangeState(State state)
        {
            Session.SetEcho(true);
            _currentState = state;

            switch (state)
            {
                case State.EnterForename:
                    Session.WriteLine($"{_forename} appears to a new name, please confirm this player's forename.\n");
                    Session.WriteLine("Please enter your character's forename: ");
                    break;

                case State.EnterPassword:
                    Session.SetEcho(false);
                    Session.WriteLine("Enter a password: ");
                    break;

                case State.SelectGender:
                    Session.WriteLine("Please select character's body type (male/female/neuter): ");
                    break;

                case State.SelectPronouns:
                    Session.WriteLine("Please select character's pronouns (masculine/feminine/neuter): ");
                    break;

                case State.EnterShortDescription:
                    Session.WriteLine("Your short description is a simple one line visual description of your");
                    Session.WriteLine("character. This is what other people see when they don't know, or haven't");
                    Session.WriteLine("remembered your name.");
                    Session.WriteLine("Don't add descriptions of clothing as what you are wearing is determined by the");
                    Session.WriteLine("in-game equipment system.");
                    Session.WriteLine("This description should be valid regardless what state you character is in, be");
                    Session.WriteLine("it awake, unconcious, asleep, angry, happy, etc.");
                    Session.WriteLine("Example: a tall, dark haired woman");
                    Session.WriteLine("Please enter your character's short description:");
                    break;

                case State.EnterDescription:
                    Session.PushState(_textEditor);
                    break;

                case State.Finished:
                    Session.PushState(new EnterWorldState(CreatePlayer()));
                    break;
            }
        }

        public void CreatePlayer()
        {
            var player = new Player()
            {
                Forename = _forename,
                PasswordHash = _password,
                ShortDescription = _shortDescription,
                Description = _description,
                Location = ServerConstants.WelcomeRoom,
                Approved = ServerConstants.AutoApprovedEnabled,
                Gender = _gender,
                Pronouns = _pronouns,
                Prompt = ">",
                Level = 0,
                Experience = 0,
                RespawnRoom = ServerConstants.StartRoom,
                Status = MobileStatus.Standing
            };

            GolemServer.Current.Database.Put(player);
        }

        public override void OnStateEnter()
        {
            switch (_currentState)
            {
                case State.EnterDescription:
                    if (!_textEditor.Success)
                    {
                        Session.WriteLine("You must give your character a description");
                        Session.PushState(_textEditor);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(_textEditor.Result))
                    {
                        Session.WriteLine("You must give your character a description");
                        Session.PushState(_textEditor);
                        return;
                    }

                    _description = _textEditor.Result;
                    ChangeState(State.EnterShortDescription);
                    break;
                default:
                    ChangeState(State.EnterForename);
                    break;
            }
        }

        public override void OnInput(string input)
        {
            switch (_currentState)
            {
                case State.EnterForename:
                    if (!PlayerLogin.ValidateUsername(input))
                    {
                        Session.WriteLine("Invalid uername");
                        ChangeState(State.EnterForename);
                        break;
                    }

                    foreName = StringHelpers.Capitalise(input);
                    ChangeState(State.EnterPassword);
                    break;

                case State.EnterPassword:
                    _password = input;
                    ChangeState(State.SelectGender);
                    break;

                case State.SelectGender:
                    if (input.ToLower() == "m" || input.ToLower() == "male")
                    {
                        _gender = PlayerGender.Male;
                        ChangeState(State.SelectPronouns);
                    }

                    else if (input.ToLower() == "f" || input.ToLower() == "female")
                    {
                        _gender = PlayerGender.Female;
                        ChangeState(State.SelectPronouns);
                    }

                    else if (input.ToLower() == "n" || input.ToLower() == "neuter")
                    {
                        _gender = PlayerGender.Neuter;
                        ChangeState(State.SelectPronouns);
                    }
                    else
                    {
                        Session.WriteLine("Unknown body type");
                        ChangeState(State.SelectGender);
                    }
                    break;

                case State.SelectPronouns:
                    if (input.ToLower() == "m" || input.ToLower() == "masculine")
                    {
                        _pronouns= PlayerPronouns.Masculine;
                        ChangeState(State.EnterDescription);
                    }

                    else if (input.ToLower() == "f" || input.ToLower() == "feminine")
                    {
                        _pronouns = PlayerPronouns.Feminine;
                        ChangeState(State.EnterDescription);
                    }

                    else if (input.ToLower() == "n" || input.ToLower() == "neuter")
                    {
                        _pronouns = PlayerPronouns.Neuter;
                        ChangeState(State.EnterDescription);
                    }
                    else
                    {
                        Session.WriteLine("Unknown pronouns");
                        ChangeState(State.SelectPronouns);
                    }
                    break;

            }
        }
    }
}