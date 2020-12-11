using System;
using Xamarin.Forms;
using MySqlConnector;

namespace Work {

    public class InsertForm : ContentPage {
        Button insertButton;
        Entry movieTitle, movieRating;
        DatePicker movieDateWhenWatch;

        void InsertButton_Click(object sender, EventArgs e) {
            if(movieTitle.Text == null || movieRating.Text == null) {
                DisplayAlert("Не все поля заполнены", "Для занесения в базу все поля должны быть заполнены", "Ok");
                return;
            }
            else if(movieTitle.Text.Length == 0 || movieRating.Text.Length == 0) {
                DisplayAlert("Не все поля заполнены", "Для занесения в базу все поля должны быть заполнены", "Ok");
                return;
            }

            try {
                using (MySqlConnection conn = DBUtils.GetDBConnection()) {
                    conn.Open();
                    InsertInDB(conn);
                }

                MessagingCenter.Send<Page>(this, "DBChange");
                Navigation.PopAsync(true);
            }
            catch (Exception ex) {
                DisplayAlert("Ошибка", ex.Message, "Ok");
            }
        }

        /// <summary>
        /// Операция вставки в БД
        /// </summary>
        /// <param name="connection">Объект соединения с БД</param>
        void InsertInDB(MySqlConnection connection) {
            try {
                using (MySqlCommand cmd = connection.CreateCommand()) {
                    string sqlRequest = "insert into movie(title, rating, when_watch) "
                                                     + " values (@title, @rating, @when_watch) ";
                    cmd.CommandText = sqlRequest;

                    cmd.Parameters.Add("@title", MySqlDbType.String).Value = movieTitle.Text;
                    if (movieRating.Text[movieRating.Text.Length - 1] == '.') {
                        movieRating.Text += "0";
                    }
                    cmd.Parameters.Add("@rating", MySqlDbType.Double).Value = double.Parse(movieRating.Text);
                    cmd.Parameters.Add("@when_watch", MySqlDbType.Date).Value = movieDateWhenWatch.Date;

                    int countAddedRows = cmd.ExecuteNonQuery();
                    if (countAddedRows >= 1) {
                        DisplayAlert("Успешно!", "Запись успешно добавлена", "Ок");
                    }
                    else {
                        DisplayAlert("Ошибка", "Произошла ошибка", "Ок");
                    }
                }
            }
            catch(Exception ex) {
                DisplayAlert("Ошибка", ex.Message, "Ok");
            }
        }

        void MovieTitle_TextChanged(object sender, TextChangedEventArgs e) {
            ((Entry)sender).Text = ((Entry)sender).Text.Length < 50 ? ((Entry)sender).Text : ((Entry)sender).Text.Remove(((Entry)sender).Text.Length - 1);
        }

        void MovieRating_TextChanged(object sender, TextChangedEventArgs e) {
            ((Entry)sender).Text = ((Entry)sender).Text.Length < 7 ? ((Entry)sender).Text : ((Entry)sender).Text.Remove(((Entry)sender).Text.Length - 1);
        }

        public InsertForm() {
            Title = "Добавить запись в БД";

            insertButton = new Button {
                Text = "Добавить",
                HorizontalOptions = LayoutOptions.Center,
            };
            insertButton.Clicked += InsertButton_Click;
            
            movieTitle = new Entry {
                Placeholder = "Название фильма..."
            };
            movieTitle.TextChanged += MovieTitle_TextChanged;

            movieRating = new Entry {
                Placeholder = "Рейтинг фильма...",
                Keyboard = Keyboard.Numeric
            };
            movieRating.TextChanged += MovieRating_TextChanged;


            Label whenWatch = new Label { Text = "Когда посмотреть?" };

            movieDateWhenWatch = new DatePicker {
                Format = "dd/MM/yyyy",
                MinimumDate = DateTime.Now.AddDays(0),
                Date = DateTime.Now.AddDays(0)
            };

            Content = new StackLayout {
                Children = {
                    new Label { Text = "Введите данные для занесения в бд:" },
                    movieTitle,
                    movieRating,
                    whenWatch,
                    movieDateWhenWatch,
                    insertButton
                }
            };
        }
    }
}