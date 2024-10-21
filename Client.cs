using System.Text;
using Newtonsoft.Json;
using MyApiApp.Models;

public class Client
{
    private static readonly HttpClient client = new HttpClient();
    private static string jwtToken = "";

    public static async Task Run()
    {
        Console.WriteLine("Добро пожаловать в телефонный справочник (консольный клиент)");

        while (true)
        {
            Console.WriteLine(new string('-', 30));
            Console.WriteLine("Выберите действие");
            Console.WriteLine(new string('-', 30));
            Console.WriteLine("Аккаунт:");
            Console.WriteLine("1 - Регистрация");
            Console.WriteLine("2 - Логин");
            Console.WriteLine("3 - Изменить пароль");
            Console.WriteLine(new string('-', 30));
            Console.WriteLine("Ваш телефонный справочник:");
            Console.WriteLine("4 - Добавить контакт");
            Console.WriteLine("5 - Посмотреть все контакты");
            Console.WriteLine("6 - Найти контакт");
            Console.WriteLine("7 - Найти контакт по имени (бинарный поиск)");
            Console.WriteLine("8 - Удалить контакт");
            Console.WriteLine(new string('-', 30));
            Console.WriteLine("История запросов:");
            Console.WriteLine("9 - Просмотр истории запросов");
            Console.WriteLine("10 - Удаление истории запросов");
            Console.WriteLine(new string('-', 30));
            Console.WriteLine("0 - Выйти");
            Console.WriteLine(new string('-', 30));

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await Register();
                    break;
                case "2":
                    await Login();
                    break;
                case "3":
                    await ChangePassword();
                    break;
                case "4":
                    await AddContact();
                    break;
                case "5":
                    await GetAllContacts();
                    break;
                case "6":
                    await SearchContacts();
                    break;
                case "7":
                    await SearchContactByName();
                    break;
                case "8":
                    await DeleteContact();
                    break;
                case "9":
                    await GetRequestHistory();
                    break;
                case "10":
                    await DeleteRequestHistory();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Неверный выбор.");
                    break;
            }
        }
    }

    // Метод регистрации
    private static async Task Register()
    {
        Console.WriteLine("Введите имя пользователя:");
        var username = Console.ReadLine();

        Console.WriteLine("Введите пароль:");
        var password = Console.ReadLine();

        if (username == null) throw new ArgumentNullException(nameof(username));
        if (password == null) throw new ArgumentNullException(nameof(password));

        var user = new User
        {
            Username = username,
            PasswordHash = password,
            Salt = Guid.NewGuid().ToString()
        };

        var json = JsonConvert.SerializeObject(user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://localhost:5194/api/Auth/register", content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic? result = JsonConvert.DeserializeObject(responseContent);
            if (result != null)
            {
                jwtToken = result.Token;

                if (jwtToken != null)
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                }

                Console.WriteLine("Регистрация успешна!");
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync(); // Добавляем вывод ошибки
            Console.WriteLine($"Ошибка регистрации: {response.StatusCode}, {errorContent}");
        }
    }

    // Метод логина
    private static async Task Login()
    {
        Console.WriteLine("Введите имя пользователя:");
        var username = Console.ReadLine();

        Console.WriteLine("Введите пароль:");
        var password = Console.ReadLine();

        if (username == null) throw new ArgumentNullException(nameof(username));
        if (password == null) throw new ArgumentNullException(nameof(password));

        var loginData = new UserLoginRequest
        {
            Username = username,
            Password = password
        };

        var json = JsonConvert.SerializeObject(loginData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://localhost:5194/api/Auth/login", content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic? result = JsonConvert.DeserializeObject(responseContent);
            if (result == null) throw new ArgumentNullException(nameof(result));
            jwtToken = result.token;

            // Проверяем на null перед использованием
            if (jwtToken != null)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
            }

            Console.WriteLine("Успешный вход!");
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync(); // Добавляем вывод ошибки
            Console.WriteLine($"Ошибка входа: {response.StatusCode}, {errorContent}");
        }
    }

    // Метод для изменения пароля
    private static async Task ChangePassword()
    {
        Console.WriteLine("Введите текущий пароль:");
        var currentPassword = Console.ReadLine();

        Console.WriteLine("Введите новый пароль:");
        var newPassword = Console.ReadLine();

        if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
        {
            Console.WriteLine("Пароль не может быть пустым.");
            return;
        }

        var passwordData = new ChangePasswordRequest
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };


        var json = JsonConvert.SerializeObject(passwordData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PatchAsync("http://localhost:5194/api/Auth/password", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Пароль успешно изменен.");
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ошибка изменения пароля: {response.StatusCode}, {errorContent}");
        }
    }

    // Метод для добавления контакта
    private static async Task AddContact()
    {
        Console.WriteLine("Введите имя контакта:");
        var name = Console.ReadLine();

        Console.WriteLine("Введите номер телефона:");
        var phoneNumber = Console.ReadLine();

        Console.WriteLine("Введите email (необязательно):");
        var email = Console.ReadLine();

        Console.WriteLine("Введите адрес (необязательно):");
        var address = Console.ReadLine();

        var contact = new Contact
        {
            Name = name ?? "Без имени",
            PhoneNumber = phoneNumber ?? "Не указан", 
            Email = string.IsNullOrWhiteSpace(email) ? "None" : email,
            Address = string.IsNullOrWhiteSpace(address) ? "None" : address
        };

        var json = JsonConvert.SerializeObject(contact);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://localhost:5194/api/contacts", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Контакт успешно добавлен.");
        }
        else
        {
            Console.WriteLine("Ошибка добавления контакта.");
        }
    }

    // Метод для получения всех контактов
    private static async Task GetAllContacts()
    {
        var response = await client.GetAsync("http://localhost:5194/api/contacts");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var contacts = JsonConvert.DeserializeObject<Contact[]>(responseContent);

            if (contacts != null)
            {
                foreach (var contact in contacts)
                {
                    Console.WriteLine($"Имя: {contact.Name}, Телефон: {contact.PhoneNumber}, Email: {contact.Email}, Адрес: {contact.Address}");
                }
            }
        }
        else
        {
            Console.WriteLine("Ошибка получения контактов.");
        }
    }

    // Метод для поиска контактов по запросу
    private static async Task SearchContacts()
    {
        Console.WriteLine("Введите поисковый запрос (имя, телефон или email):");
        var query = Console.ReadLine();

        var json = JsonConvert.SerializeObject(query);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://localhost:5194/api/contacts/search", content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var contacts = JsonConvert.DeserializeObject<Contact[]>(responseContent);

            if (contacts != null)
            {
                foreach (var contact in contacts)
                {
                    Console.WriteLine($"Имя: {contact.Name}, Телефон: {contact.PhoneNumber}, Email: {contact.Email}, Адрес: {contact.Address}");
                }
            }
        }
        else
        {
            Console.WriteLine("Ошибка поиска контактов.");
        }
    }

    // Метод для поиска контакта по имени с использованием бинарного поиска
    private static async Task SearchContactByName()
    {
        var contacts = await GetAllContactsArray();

        Console.WriteLine("Введите имя контакта для поиска:");
        var nameToSearch = Console.ReadLine();

        if (string.IsNullOrEmpty(nameToSearch))
        {
            Console.WriteLine("Имя для поиска не может быть пустым.");
            return;
        }

        var index = BinarySearch(contacts, nameToSearch);


        if (index != -1)
        {
            var contact = contacts[index];
            Console.WriteLine($"Найден контакт: Имя: {contact.Name}, Телефон: {contact.PhoneNumber}, Email: {contact.Email}, Адрес: {contact.Address}");
        }
        else
        {
            Console.WriteLine("Контакт не найден.");
        }
    }

    // Метод для получения всех контактов как массив
    private static async Task<Contact[]> GetAllContactsArray()
    {
        var response = await client.GetAsync("http://localhost:5194/api/contacts");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var contacts = JsonConvert.DeserializeObject<Contact[]>(responseContent);

            if (contacts == null || contacts.Length == 0)
            {
                Console.WriteLine("Список контактов пуст.");
                return Array.Empty<Contact>();
            }

            Array.Sort(contacts, (x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));

            return contacts;
        }
        else
        {
            Console.WriteLine("Ошибка получения контактов.");
            return Array.Empty<Contact>();
        }
    }

    // Метод бинарного поиска
    private static int BinarySearch(Contact[] contacts, string name)
    {
        int left = 0;
        int right = contacts.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            
            int comparison = string.Compare(contacts[mid].Name, name, StringComparison.OrdinalIgnoreCase);

            if (comparison == 0)
            {
                return mid;
            }
            else if (comparison < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return -1;
    }

    // Метод для удаления контакта
    private static async Task DeleteContact()
    {
        Console.WriteLine("Введите ID контакта, который хотите удалить:");
        var id = Console.ReadLine();

        var response = await client.DeleteAsync($"http://localhost:5194/api/contacts/{id}");

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Контакт успешно удален.");
        }
        else
        {
            Console.WriteLine("Ошибка удаления контакта.");
        }
    }
    
    // Метод для показа истории запросов
    public static async Task GetRequestHistory()
    {
        var response = await client.GetAsync("http://localhost:5194/api/RequestHistory");

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var history = JsonConvert.DeserializeObject<List<History>>(responseContent);
            
            if (history != null && history.Any())
            {
                Console.WriteLine("История запросов:");
                foreach (var record in history)
                {
                    Console.WriteLine($"ID: {record.Id}, Метод: {record.HttpMethod}, Путь: {record.Path}, Время: {record.Timestamp}, Query: {record.QueryString}, Body: {record.BodyContent}");
                }
            }
            else
            {
                Console.WriteLine("История запросов пуста.");
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ошибка при получении истории: {response.StatusCode}, {errorContent}");
        }
    }

    // Метод для удаления истории запросов
    public static async Task DeleteRequestHistory()
    {
        var response = await client.DeleteAsync("http://localhost:5194/api/RequestHistory");

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("История запросов успешно удалена.");
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ошибка при удалении истории: {response.StatusCode}, {errorContent}");
        }
    }

}