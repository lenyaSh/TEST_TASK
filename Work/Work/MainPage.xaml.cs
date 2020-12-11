using System;
using Xamarin.Forms;
using MySqlConnector;

namespace Work {
    public partial class MainPage : ContentPage {

        TableView db = null;
        public MainPage() {
            InitializeComponent();

            MakePage();
            ChangeMessage();
        }

        /// <summary>
        /// Создание контента на главной странице
        /// </summary>
        void MakePage() {
            Button Insert = new Button {
                Text = "Добавить"
            };
            Insert.Clicked += InsertIntoDB;

            Button Change = new Button {
                Text = "Изменить"
            };
            Change.Clicked += ChangeFields;

            Button Delete = new Button {
                Text = "Удалить"
            };
            Delete.Clicked += DeleteFromDB;

            if (db != null) {
                Content = new StackLayout {
                    Children = {
                        db,
                        Insert,
                        Change,
                        Delete
                    }
                };
            }
        }


        async void ChangeFields(object sende, EventArgs e) {
            await Navigation.PushAsync(new PageChangeFieldDB(), true);
        }

        /// <summary>
        /// Формирование таблицы из данных в БД
        /// </summary>
        /// <param name="connection">Объект соединения с БД</param>
        /// <returns>Заполнененная таблица или null при ошибке</returns>
        TableView FillTable(MySqlConnection connection) {
            TableView table = new TableView {
                Intent = TableIntent.Data,
                Root = new TableRoot("Данные, полученные из БД")
            };

            try {
                using (MySqlCommand cmd = new MySqlCommand()) {
                    cmd.Connection = connection;
                    cmd.CommandText = "select title, rating, when_watch from movie;";
                    using (MySqlDataReader reader = cmd.ExecuteReader()) {
                        if (reader.HasRows) {
                            string changingTitle = "", changingRating = "", changingDate = "";

                            for (int i = 1; reader.Read(); i++) {
                                changingTitle = reader.GetString("title").ToString();
                                table.Root.Add(new TableSection("Название:") {
                                        new TextCell {
                                            Text = changingTitle,
                                            TextColor = Color.Black
                                        },
                                    }
                                );

                                changingRating = reader.GetDouble("rating").ToString();
                                table.Root.Add(new TableSection("Рейтинг:") {
                                        new TextCell {
                                            Text = changingRating,
                                            TextColor = Color.Black
                                        }
                                    }
                                );

                                changingDate = reader.GetDateTime("when_watch").ToString("dd/MM/yyyy");
                                table.Root.Add(new TableSection("Когда посмотреть:") {
                                    new TextCell {
                                        Text = changingDate,
                                        TextColor = Color.Black
                                    }
                                }
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                DisplayAlert("Ошибка", ex.Message, "Ok");
                return null;
            }

            return table;
        }

        /// <summary>
        /// Переход на страницу с добавлением строки в БД
        /// </summary>
        async void InsertIntoDB(object sender, EventArgs e) {
            await Navigation.PushAsync(new InsertForm());
        }

        /// <summary>
        /// Удаление строки из таблицы
        /// </summary>
        async void DeleteFromDB(object sender, EventArgs e) {
            await Navigation.PushAsync(new PageDeleteFromDB());
        }

        private void ChangeMessage() {
            MessagingCenter.Subscribe<Page>(
                this,
                "DBChange",
                (sender) => { ConnectWithDataBase(null, null); });
        }

        void ConnectWithDataBase(object sender, EventArgs e) {
            try {
                using (MySqlConnection conn = DBUtils.GetDBConnection()) {
                    conn.Open();
                    db = FillTable(conn);
                }
            }
            catch (Exception ex) {
                DisplayAlert("Ошибка", ex.Message, "Ok");
            }

            MakePage();
        }
    }
}
