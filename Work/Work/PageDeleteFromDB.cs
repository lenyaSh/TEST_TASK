using System;
using Xamarin.Forms;
using MySqlConnector;

namespace Work {
    public class PageDeleteFromDB : ContentPage {
        Entry deleteTitle;

        void CheckLength(object sender, EventArgs e) {
            ((Entry)sender).Text = ((Entry)sender).Text.Length < 50 ? ((Entry)sender).Text : ((Entry)sender).Text.Remove(((Entry)sender).Text.Length - 1);
        }

        /// <summary>
        /// Выполнение запроса удаления из БД
        /// </summary>
        /// <param name="connection">Объект соединения</param>
        void RequestToDB(MySqlConnection connection) {
            try {
                using (MySqlCommand cmd = connection.CreateCommand()) {
                    string sqlRequest = "delete from movie where title = @movieTitle";
                    cmd.Parameters.Add("@movieTitle", MySqlDbType.String).Value = deleteTitle.Text;
                    cmd.CommandText = sqlRequest;

                    int countAddedRows = cmd.ExecuteNonQuery();
                    if (countAddedRows >= 1) {
                        DisplayAlert("Успешно!", "Фильм удален", "Ок");
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
        /// Формирование соединения и выполнения запроса по удалению из БД
        /// </summary>
        void DeleteItem(object sender, EventArgs e) {
            if (deleteTitle.Text == null) {
                DisplayAlert("Поле не заполнено", "Для занесения в базу поле должно быть заполнено", "Ok");
                return;
            }
            else if (deleteTitle.Text.Length == 0) {
                DisplayAlert("Поле не заполнено", "Для занесения в базу поле должно быть заполнено", "Ok");
                return;
            }

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

        public PageDeleteFromDB() {
            Button deleteItem = new Button {
                Text = "Удалить",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.End
            };
            deleteItem.Clicked += DeleteItem;

            deleteTitle = new Entry {
                Placeholder = "Название фильма...",
                Keyboard = Keyboard.Text
            };
            deleteTitle.TextChanged += CheckLength;

            Content = new StackLayout {
                Children = {
                    new Label {
                        Text = "Введите название фильма, который хотите удалить",
                        VerticalTextAlignment = TextAlignment.Start,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    deleteTitle,
                    deleteItem
                }
            };
        }
    }
}