using Microsoft.EntityFrameworkCore;
using QuizGame.Models;

namespace QuizGame.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        if (context.Categories.Any()) return;

        var science = new Category
        {
            Name = "Science",
            Description = "Physics, chemistry, biology, and space-themed trivia challenges."
        };
        var history = new Category
        {
            Name = "History",
            Description = "Major world events, timelines, and iconic historical figures."
        };
        var egyptianFootball = new Category
        {
            Name = "Egyptian Football",
            Description = "Egyptian clubs, national team milestones, and iconic players."
        };
        var geography = new Category
        {
            Name = "Geography",
            Description = "Countries, capitals, landmarks, and world map fundamentals."
        };

        context.Categories.AddRange(science, history, egyptianFootball, geography);
        await context.SaveChangesAsync();

        Question BuildQuestion(
            int categoryId,
            string text,
            string optionA,
            string optionB,
            string optionC,
            string optionD,
            int correctOption,
            int points = 100,
            int timeLimitSeconds = 20)
        {
            return new Question
            {
                CategoryId = categoryId,
                Text = text,
                Points = points,
                TimeLimitSeconds = timeLimitSeconds,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = optionA, IsCorrect = correctOption == 1 },
                    new() { Text = optionB, IsCorrect = correctOption == 2 },
                    new() { Text = optionC, IsCorrect = correctOption == 3 },
                    new() { Text = optionD, IsCorrect = correctOption == 4 },
                }
            };
        }

        var questions = new List<Question>
        {
            // Science (10)
            BuildQuestion(science.Id, "What planet is closest to the Sun?", "Mercury", "Venus", "Earth", "Mars", 1, 100, 20),
            BuildQuestion(science.Id, "What is the chemical symbol for water?", "CO2", "NaCl", "H2O", "O2", 3, 100, 15),
            BuildQuestion(science.Id, "Which planet is known as the Red Planet?", "Jupiter", "Mars", "Saturn", "Neptune", 2, 100, 20),
            BuildQuestion(science.Id, "What gas do plants primarily absorb from the atmosphere?", "Oxygen", "Nitrogen", "Carbon Dioxide", "Hydrogen", 3, 120, 20),
            BuildQuestion(science.Id, "What is the center of an atom called?", "Electron", "Nucleus", "Proton", "Orbit", 2, 120, 20),
            BuildQuestion(science.Id, "What is the approximate speed of light in vacuum?", "300,000 km/s", "30,000 km/s", "3,000 km/s", "3,000,000 km/s", 1, 150, 25),
            BuildQuestion(science.Id, "What is the hardest natural substance?", "Iron", "Quartz", "Diamond", "Granite", 3, 120, 20),
            BuildQuestion(science.Id, "What is the largest organ in the human body?", "Liver", "Skin", "Lung", "Heart", 2, 120, 20),
            BuildQuestion(science.Id, "At sea level, water boils at what temperature?", "90 C", "95 C", "100 C", "110 C", 3, 100, 15),
            BuildQuestion(science.Id, "Who discovered penicillin?", "Isaac Newton", "Alexander Fleming", "Louis Pasteur", "Marie Curie", 2, 150, 25),

            // History (10)
            BuildQuestion(history.Id, "In what year did World War II end?", "1943", "1944", "1945", "1947", 3, 150, 20),
            BuildQuestion(history.Id, "Who was the first U.S. President?", "Thomas Jefferson", "George Washington", "John Adams", "Abraham Lincoln", 2, 100, 15),
            BuildQuestion(history.Id, "The Great Wall is located in which country?", "India", "Mongolia", "China", "Japan", 3, 100, 15),
            BuildQuestion(history.Id, "The Berlin Wall fell in which year?", "1987", "1989", "1991", "1993", 2, 130, 20),
            BuildQuestion(history.Id, "What writing system was used in ancient Egypt?", "Cuneiform", "Hieroglyphics", "Latin", "Runes", 2, 120, 20),
            BuildQuestion(history.Id, "In which country did the Renaissance begin?", "France", "England", "Italy", "Spain", 3, 130, 20),
            BuildQuestion(history.Id, "In what year did the Titanic sink?", "1909", "1912", "1918", "1921", 2, 120, 20),
            BuildQuestion(history.Id, "Who was the first human to walk on the moon?", "Yuri Gagarin", "Buzz Aldrin", "Neil Armstrong", "Michael Collins", 3, 120, 20),
            BuildQuestion(history.Id, "The Magna Carta was signed in which year?", "1066", "1215", "1492", "1776", 2, 150, 25),
            BuildQuestion(history.Id, "Who led the non-violent movement for Indian independence?", "Subhas Chandra Bose", "Jawaharlal Nehru", "Mahatma Gandhi", "Bhagat Singh", 3, 130, 20),

            // Egyptian Football (10)
            BuildQuestion(egyptianFootball.Id, "Which club has won the most Egyptian Premier League titles?", "Zamalek", "Al Ahly", "Pyramids", "Ismaily", 2, 150, 20),
            BuildQuestion(egyptianFootball.Id, "The Cairo Derby is mainly played between which two clubs?", "Al Ahly and Ismaily", "Al Ahly and Zamalek", "Zamalek and Al Masry", "Pyramids and Zamalek", 2, 130, 20),
            BuildQuestion(egyptianFootball.Id, "What is the nickname of the Egypt national football team?", "The Lions", "The Eagles", "The Pharaohs", "The Warriors", 3, 100, 15),
            BuildQuestion(egyptianFootball.Id, "How many times has Egypt won the Africa Cup of Nations?", "5", "6", "7", "8", 3, 170, 25),
            BuildQuestion(egyptianFootball.Id, "Which Egyptian player is nicknamed \"The Egyptian King\"?", "Mohamed Elneny", "Ahmed Hegazy", "Omar Marmoush", "Mohamed Salah", 4, 100, 15),
            BuildQuestion(egyptianFootball.Id, "Which Cairo club is commonly associated with the color white?", "Al Ahly", "ENPPI", "Zamalek", "Future FC", 3, 100, 15),
            BuildQuestion(egyptianFootball.Id, "What is the top football division in Egypt called?", "Egypt Cup", "Egyptian Premier League", "Nile League 2", "Cairo League", 2, 120, 20),
            BuildQuestion(egyptianFootball.Id, "Al Masry SC is based in which Egyptian city?", "Alexandria", "Port Said", "Suez", "Mansoura", 2, 130, 20),
            BuildQuestion(egyptianFootball.Id, "Which stadium is most commonly used for major Egypt national team matches in Cairo?", "Alexandria Stadium", "30 June Stadium", "Cairo International Stadium", "El Mahalla Stadium", 3, 140, 20),
            BuildQuestion(egyptianFootball.Id, "Egypt hosted the Africa Cup of Nations in which recent year?", "2013", "2015", "2017", "2019", 4, 120, 20),

            // Geography (10)
            BuildQuestion(geography.Id, "What is the largest ocean on Earth?", "Atlantic Ocean", "Indian Ocean", "Pacific Ocean", "Arctic Ocean", 3, 100, 15),
            BuildQuestion(geography.Id, "Which river is traditionally considered the longest in the world?", "Amazon", "Yangtze", "Nile", "Mississippi", 3, 120, 20),
            BuildQuestion(geography.Id, "What is the capital city of Australia?", "Sydney", "Melbourne", "Perth", "Canberra", 4, 130, 20),
            BuildQuestion(geography.Id, "Mount Fuji is located in which country?", "China", "South Korea", "Japan", "Thailand", 3, 100, 15),
            BuildQuestion(geography.Id, "Which desert covers much of North Africa?", "Gobi", "Kalahari", "Arabian", "Sahara", 4, 100, 15),
            BuildQuestion(geography.Id, "What is the capital of Canada?", "Toronto", "Vancouver", "Montreal", "Ottawa", 4, 120, 20),
            BuildQuestion(geography.Id, "Brazil is located on which continent?", "North America", "South America", "Europe", "Africa", 2, 100, 15),
            BuildQuestion(geography.Id, "Which river runs through Paris?", "Danube", "Rhine", "Seine", "Thames", 3, 120, 20),
            BuildQuestion(geography.Id, "What is the highest mountain above sea level?", "K2", "Kangchenjunga", "Lhotse", "Mount Everest", 4, 120, 20),
            BuildQuestion(geography.Id, "What is the smallest country in the world by area?", "Monaco", "San Marino", "Vatican City", "Liechtenstein", 3, 130, 20),
        };

        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();
    }
}