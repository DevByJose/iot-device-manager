using System;
using System.Collections.Generic;
using UMLIoT.Core.Devices;
using UMLIoT.Patterns.Facade;
using UMLIoT.Patterns.Observer;

namespace UMLIoT
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var facade = new IoTFacade();

            var deviceEventManager = new DeviceEventManager();
            deviceEventManager.addObserver(new MobileNotifier());
            deviceEventManager.addObserver(new SecuritySystem());
            deviceEventManager.addObserver(new EventLogger());
            facade.setEventManager(deviceEventManager);

            bool salir = false;

            while (!salir)
            {
                Console.WriteLine("\n==== SISTEMA IOT ====");
                Console.WriteLine("1. Registrar usuario");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Registrar dispositivo");
                Console.WriteLine("4. Ver dispositivos");
                Console.WriteLine("5. Turn on device");
                Console.WriteLine("6. Turn off device");
                Console.WriteLine("7. Activate alarm");
                Console.WriteLine("8. Start recording");
                Console.WriteLine("9. Remove device");
                Console.WriteLine("10. Get device status");
                Console.WriteLine("11. Logout");
                Console.WriteLine("0. Salir");
                Console.Write("Seleccione una opción: ");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        Console.Write("Nombre: ");
                        var name = Console.ReadLine() ?? string.Empty;
                        Console.Write("Email: ");
                        var email = Console.ReadLine() ?? string.Empty;
                        Console.Write("Password: ");
                        var password = Console.ReadLine() ?? string.Empty;

                        var user = facade.registerUser(name, email, password);
                        Console.WriteLine($"Usuario registrado con id {user.GetId()}");
                        break;

                    case "2":
                        Console.Write("Email: ");
                        var loginEmail = Console.ReadLine() ?? string.Empty;
                        Console.Write("Password: ");
                        var loginPassword = Console.ReadLine() ?? string.Empty;
                        Console.WriteLine(facade.login(loginEmail, loginPassword)
                            ? "Login exitoso"
                            : "Credenciales inválidas");
                        break;

                    case "3":
                        if (!facade.isUserLoggedIn())
                        {
                            Console.WriteLine("Debe iniciar sesión para registrar un dispositivo.");
                            break;
                        }

                        Console.Write("Tipo de dispositivo (camera/smartlight/alarm): ");
                        var type = (Console.ReadLine() ?? string.Empty).Trim();
                        var config = new Dictionary<string, string>();

                        Console.Write("Name: ");
                        config["name"] = Console.ReadLine() ?? string.Empty;

                        if (type.Equals("smartlight", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.Write("Color: ");
                            config["color"] = Console.ReadLine() ?? string.Empty;
                            Console.Write("Schedule: ");
                            config["schedule"] = Console.ReadLine() ?? string.Empty;
                        }

                        var registered = facade.registerDevice(type, config);
                        if (registered is null)
                        {
                            Console.WriteLine("No se pudo crear el dispositivo");
                        }
                        else
                        {
                            Console.WriteLine("Dispositivo registrado");
                            if (registered is Device dev && type.Equals("camera", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"IP Asignada a la Cámara: {dev.getIPAddress()}");
                            }
                        }
                        break;

                    case "4":
                        var devices = facade.getAllDevice();
                        if (devices.Count == 0)
                        {
                            Console.WriteLine("No hay dispositivos registrados");
                        }
                        else
                        {
                            foreach (var device in devices)
                            {
                                if (device is Device concrete)
                                {
                                    Console.WriteLine($"Id: {concrete.getID()} | Tipo: {device.GetType().Name} | Name: {concrete.getName()} | IP: {concrete.getIPAddress()} | Status: {device.getStatus().GetType().Name}");
                                }
                                else
                                {
                                    Console.WriteLine($"Id: -1 | Tipo: {device.GetType().Name} | Name:  | Status: {device.getStatus().GetType().Name}");
                                }
                            }
                        }
                        break;

                    case "5":
                        Console.Write("Device id: ");
                        facade.turnOnDevice(int.Parse(Console.ReadLine() ?? "0"));
                        break;

                    case "6":
                        Console.Write("Device id: ");
                        facade.turnOffDevice(int.Parse(Console.ReadLine() ?? "0"));
                        break;

                    case "7":
                        Console.Write("Device id: ");
                        facade.activateAlarm(int.Parse(Console.ReadLine() ?? "0"));
                        break;

                    case "8":
                        Console.Write("Device id: ");
                        facade.startRecording(int.Parse(Console.ReadLine() ?? "0"));
                        break;

                    case "9":
                        Console.Write("Device id: ");
                        Console.WriteLine(facade.removeDevice(int.Parse(Console.ReadLine() ?? "0"))
                            ? "Dispositivo eliminado"
                            : "No se encontró el dispositivo");
                        break;

                    case "10":
                        Console.Write("Device id: ");
                        Console.WriteLine(facade.getDeviceStatus(int.Parse(Console.ReadLine() ?? "0")));
                        break;

                    case "11":
                        Console.Write("User id: ");
                        facade.logout(int.Parse(Console.ReadLine() ?? "0"));
                        Console.WriteLine("Logout ejecutado");
                        break;

                    case "0":
                        salir = true;
                        break;

                    default:
                        Console.WriteLine("Opción no válida");
                        break;
                }
            }
        }
    }   
}
