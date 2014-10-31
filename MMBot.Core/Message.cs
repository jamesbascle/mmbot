namespace MMBot
{
    public class Message
    {
        public IUser User { get; protected set; }

        public bool Done { get; set; }

        public void Finish()
        {
            Done = true;
        }
    }

    public class TextMessage : Message
    {
        public TextMessage(IUser user, string text)
        {
            User = user;
            Text = text;
        }

        public string Text { get; private set; }
    }

    public class EnterMessage : Message
    {
        public EnterMessage(IUser user)
        {
            User = user;
        }
    }

    public class LeaveMessage : Message
    {
        public LeaveMessage(IUser user)
        {
            User = user;
        }
    }

    public class TopicMessage : Message
    {
        public TopicMessage(IUser user, string topic)
        {
            User = user;
            Topic = topic;
        }

        public string Topic { get; private set; }
    }

    public class CatchAllMessage : Message
    {
        public CatchAllMessage(IUser user, string textData)
        {
            User = user;
            Text = textData;
        }

        public string Text { get; private set; }
    }
}