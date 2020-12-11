using System;
using System.Linq;
using Xamarin.Forms;
using MySqlConnector;

namespace Work {
    public class PageChangeFieldDB : ContentPage {
        Entry changeTitle, changeRating, oldMovieTitle;
        DatePicker changeWhenWatch;
        string notChangedMovieTitle;

        void CheckLength(object sender, EventArgs e) {
            ((Entry)sender).Text = ((Entry)sender).Text.Length < 50 ? ((Entry)sender).Text : ((Entry)sender).Text.Remove(((Entry)sender).Text.Length - 1);
        }
        void CheckRatingLength(object sender, EventArgs e) {
            ((Entry)sender).Text = ((Entry)sender).Text.Length < 7 ? ((Entry)sender).Text : ((Entry)sender).Text.Remove(((Entry)sender).Text.Length - 1);
        }

        /// <summary>
        /// Проверка перед занесением в БД сколько введено полей для изменения
        /// </summary>
        /// <returns>1 - нужно ли изменять название фильма, 2 - нужно ли изменять его рейтинг
        /// 3 - нужно ли изменять его дату просмотра</returns>
        bool[] HowManyFieldsChanged() {
            bool[] countField = {true, true, true};

            countField[0] = changeTitle.Text == null || changeTitle.Text.Length == 0 ? false : true;
            countField[1] = changeRating.Text == null || changeRating.Text.Length == 0 ? false : true;
            countField[2] = changeWhenWatch.Date == null ? false : true;

            return countField;
        }

        /// <summary>
        /// Выполняется запрос на изменение в БД
        /// </summary>
        /// <param name="connection">объект соединения</param>
        void RequestToDB(MySqlConnection connection) {
            bool[] countChangingFields = HowManyFieldsChanged();
            if(!countChangingFields[0] && !countChangingFields[1] && !countChangingFields[2]) {
                throw new Exception("Хотя бы одно поле должно быть заполнено");
            }

            try {
                using (MySqlCommand cmd = connection.CreateCommand()) {
                    string sqlRequest = "update movie set ";
                    if (countChangingFields[0]) {
                        sqlRequest += "title = @newMovieTitle, ";
                        cmd.Parameters.Add("@newMovieTitle", MySqlDbType.String).Value = changeTitle.Text;
                    }
                    if(countChangingFields[1]) {
                        sqlRequest += " rating = @newMovieRating, ";
                        if (changeRating.Text[changeRating.Text.Length - 1] == '.') {
                            changeRating.Text += "0";
                        }
                        cmd.Parameters.Add("@newMovieRating", MySqlDbType.Double).Value = changeRating.Text;
                    }
                    if (countChangingFields[2]) {
                        sqlRequest += " when_watch = @newDateWatch ";
                        cmd.Parameters.Add("@newDateWatch", MySqlDbType.Date).Value = changeWhenWatch.Date;
                    }
                    sqlRequest += "where title = @movieTitle";
                    cmd.Parameters.Add("@movieTitle", MySqlDbType.String).Value = notChangedMovieTitle;
                    cmd.CommandText = sqlRequest;

                    int countAddedRows = cmd.ExecuteNonQuery();
                    if (countAddedRows >= 1) {
                        DisplayAlert("Успешно!", "Значение изменено", "Ок");
                    }
                    else {
                        DisplayAlert("Ошибка", "Такого фильма в базе нет", "Ок");
                    }
                }
            }
            catch (Exception ex) {
                DisplayAlert("Ошибка", ex.Message, "Ok");
            }
        }

        /// <summary>
        /// Соединение с бд и выполнение запроса
        /// </summary>
        void MakeConnection(object sender, EventArgs e) {
            try {
                using (MySqlConnection conn = DBUtils.GetDBConnection()) {
                    conn.Open();
                    RequestToDB(conn);
                }

                MessagingCenter.Send<Page>(this, "DBChange");
                Navigation.PopAsync(true);
            }
            catch (Exception ex) {
                DisplayAlert("Ошибка", ex.Message, "Ок");
            }
        }

        /// <summary>
        /// Формирование страницы с полями для новых данных
        /// </summary>
        void InputNewData() {
            changeTitle = new Entry {
                Placeholder = "Новое название фильма...",
                Keyboard = Keyboard.Text
            };
            changeTitle.TextChanged += CheckLength;

            changeRating = new Entry {
                Placeholder = "Новый рейтинг...",
                Keyboard = Keyboard.Numeric
            };
            changeRating.TextChanged += CheckRatingLength;

            changeWhenWatch = new DatePicker {
                Format = "dd/MM/yyyy",
                MinimumDate = DateTime.Now.AddDays(0)
            };

            Button change = new Button {
                Text = "Изменить",
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };
            change.Clicked += MakeConnection;

            Content = new StackLayout {
                Children = {
                    new Label {
                        Text = "Введите новые данные",
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    changeTitle,
                    changeRating,
                    changeWhenWatch,
                    change
                }
            };
        }

        void ShowChangeFields(object sender, EventArgs e) {
            if (oldMovieTitle.Text == null) {
                DisplayAlert("Поле не заполнено", "Для продолжения нужно ввести название уже добавленного фильма", "Ok");
                return;
            }
            else if (oldMovieTitle.Text.Length == 0) {
                DisplayAlert("Поле не заполнено", "Для продолжения нужно ввести название уже добавленного фильма", "Ok");
                return;
            }
            notChangedMovieTitle = oldMovieTitle.Text;

            InputNewData();
        }

        /// <summary>
        /// формирует первый шаг для изменения информации фильма - ввод его названия
        /// </summary>
        void InputMovieTitle() {
            Button NextStep = new Button {
                Text = "Далее",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.End
            };
            NextStep.Clicked += ShowChangeFields;

            oldMovieTitle = new Entry {
                Placeholder = "Название фильма...",
                Keyboard = Keyboard.Text
            };
            oldMovieTitle.TextChanged += CheckLength;

            Content = new StackLayout {
                Children = {
                    new Label {
                        Text = "Введите название фильма, значения которого хотите поменять",
                        VerticalTextAlignment = TextAlignment.Start,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    oldMovieTitle,
                    NextStep
                }
            };
        }

        public PageChangeFieldDB() {
            InputMovieTitle();
        }
    }
}