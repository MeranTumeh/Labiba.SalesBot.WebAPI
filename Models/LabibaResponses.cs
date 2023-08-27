using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Labiba.Sales.WebAPI.Models
{
    public class LabibaResponses
    {
        public class HeroCardsModel
        {
            public class Image
            {
                public string URL { get; set; } = string.Empty;
            }

            public class Button
            {
                public string Title { get; set; }
                public string Type { get; set; }
                public string Value { get; set; }
                public string entityKeyword { get; set; }

                public static implicit operator List<object>(Button v)
                {
                    throw new NotImplementedException();
                }
            }
            public class hero_cards
            {
                public string Title { get; set; }
                public string Subtitle { get; set; }
                public string Text { get; set; }
                public List<Image> Images { get; set; } = new List<Image>();
                public List<Button> Buttons { get; set; } = new List<Button>();
            }

            public class RootObject
            {

                public string response { get; set; }

                public string success_message { get; set; }

                public string failure_message { get; set; }

                public List<hero_cards> hero_cards { get; set; } = new List<hero_cards>();

            }
        }

        public class StateModel
        {
            public string state { get; set; }
            public string SlotFillingState { get; set; }
        }

        public class TextModel
        {
            public string text { get; set; }
            public string slotFillingText { get; set; }
        }
    }
}
