namespace Ticketbooth.Scanner.Domain.Data.Models
{
    public class SeatModel
    {
        public SeatModel()
        {
        }

        public SeatModel(int number, char letter)
        {
            Number = number;
            Letter = letter;
        }

        public int Number { get; set; }

        public char Letter { get; set; }
    }
}
