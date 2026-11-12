using System;
using System.Collections.Generic;
using System.Globalization;

class Categoria
{
    public int Id { get; private set; }
    public string Nombre { get; set; }

    public Categoria(int id, string nombre)
    {
        Id = id;
        Nombre = nombre;
    }

    public override string ToString()
    {
        return Id + ". " + Nombre;
    }
}

abstract class Movimiento
{
    public string Descripcion { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }

    protected Movimiento(string descripcion, decimal monto, DateTime fecha)
    {
        Descripcion = descripcion;
        Monto = monto;
        Fecha = fecha;
    }

    public bool CoincideDescripcion(string texto)
    {
        return Descripcion.ToLower().Contains(texto);
    }

    public bool MontoEnRango(decimal minimo, decimal maximo)
    {
        return Monto >= minimo && Monto <= maximo;
    }

    public bool EsDeFecha(DateTime fecha)
    {
        return Fecha.Date == fecha;
    }
}

class Gasto : Movimiento
{
    public Categoria Categoria { get; set; }

    public Gasto(string descripcion, decimal monto, Categoria categoria, DateTime fecha)
        : base(descripcion, monto, fecha)
    {
        Categoria = categoria;
    }

    public bool EsDeCategoria(int idCategoria)
    {
        return Categoria.Id == idCategoria;
    }

    public override string ToString()
    {
        return Fecha.ToString("dd/MM/yyyy")
            + " | " + Descripcion
            + " | " + Monto.ToString("N2")
            + " | " + Categoria.Nombre;
    }
}

class Ingreso : Movimiento
{
    public Ingreso(string descripcion, decimal monto, DateTime fecha)
        : base(descripcion, monto, fecha)
    {
    }

    public override string ToString()
    {
        return Fecha.ToString("dd/MM/yyyy")
            + " | " + Descripcion
            + " | " + Monto.ToString("N2");
    }
}

class RegistroFinanciero
{
    public Dictionary<int, Categoria> Categorias { get; } = new Dictionary<int, Categoria>();
    public List<Gasto> Gastos { get; } = new List<Gasto>();
    public List<Ingreso> Ingresos { get; } = new List<Ingreso>();

    private int siguienteCategoria = 1;

    public RegistroFinanciero()
    {
        CargarCategoriasIniciales();
    }

    private void CargarCategoriasIniciales()
    {
        string[] iniciales = { "Comida", "Transporte", "Servicios", "Ocio", "Salud" };

        foreach (string nombre in iniciales)
        {
            CrearCategoria(nombre);
        }
    }

    public Categoria CrearCategoria(string nombre)
    {
        Categoria categoria = new Categoria(siguienteCategoria, nombre);
        Categorias.Add(siguienteCategoria, categoria);
        siguienteCategoria++;
        return categoria;
    }

    public bool ExisteNombreCategoria(string nombre, int ignorarId)
    {
        foreach (Categoria existente in Categorias.Values)
        {
            if (existente.Id != ignorarId
                && existente.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public bool CategoriaEnUso(int idCategoria)
    {
        foreach (Gasto gasto in Gastos)
        {
            if (gasto.EsDeCategoria(idCategoria))
            {
                return true;
            }
        }

        return false;
    }

    public decimal TotalIngresos()
    {
        decimal total = 0;

        foreach (Ingreso ingreso in Ingresos)
        {
            total += ingreso.Monto;
        }

        return total;
    }

    public decimal TotalGastos()
    {
        decimal total = 0;

        foreach (Gasto gasto in Gastos)
        {
            total += gasto.Monto;
        }

        return total;
    }

    public decimal SubtotalCategoria(int idCategoria)
    {
        decimal subtotal = 0;

        foreach (Gasto gasto in Gastos)
        {
            if (gasto.EsDeCategoria(idCategoria))
            {
                subtotal += gasto.Monto;
            }
        }

        return subtotal;
    }
}

static class Entrada
{
    public static string LeerLinea()
    {
        string? linea = Console.ReadLine();

        if (linea == null)
        {
            Console.WriteLine("\nEntrada cerrada. Saliendo del programa.");
            Environment.Exit(0);
        }

        return linea;
    }

    public static int LeerEntero(string mensaje)
    {
        while (true)
        {
            Console.Write(mensaje);
            int valor;

            if (int.TryParse(LeerLinea(), out valor))
            {
                return valor;
            }

            Console.WriteLine("Debe escribir un número entero.");
        }
    }

    public static bool TryParseMonto(string texto, out decimal valor)
    {
        texto = texto.Trim().Replace(',', '.');

        return decimal.TryParse(texto, NumberStyles.Number,
            CultureInfo.InvariantCulture, out valor) && valor > 0;
    }

    public static decimal LeerMonto(string mensaje)
    {
        while (true)
        {
            Console.Write(mensaje);
            decimal valor;

            if (TryParseMonto(LeerLinea(), out valor))
            {
                return valor;
            }

            Console.WriteLine("Debe escribir un monto numérico mayor que cero.");
        }
    }

    public static string LeerTexto(string mensaje)
    {
        while (true)
        {
            Console.Write(mensaje);
            string texto = LeerLinea().Trim();

            if (texto != "")
            {
                return texto;
            }

            Console.WriteLine("Este campo no puede estar vacío.");
        }
    }

    public static DateTime LeerFecha(string mensaje)
    {
        while (true)
        {
            Console.Write(mensaje);
            DateTime fecha;

            if (DateTime.TryParseExact(LeerLinea().Trim(), "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                return fecha.Date;
            }

            Console.WriteLine("Fecha inválida. Use el formato dd/mm/aaaa.");
        }
    }

    public static bool Confirmar(string mensaje)
    {
        Console.Write(mensaje);
        return LeerLinea().Trim().ToLower() == "s";
    }
}

class Aplicacion
{
    private RegistroFinanciero registro = new RegistroFinanciero();

    public void Ejecutar()
    {
        bool salir = false;

        while (!salir)
        {
            try
            {
                Console.WriteLine("\n===== REGISTRO DE GASTOS PERSONALES =====");
                Console.WriteLine("1. Gastos");
                Console.WriteLine("2. Ingresos");
                Console.WriteLine("3. Categorías");
                Console.WriteLine("4. Balance");
                Console.WriteLine("0. Salir");

                switch (Entrada.LeerEntero("Opción: "))
                {
                    case 1: MenuGastos(); break;
                    case 2: MenuIngresos(); break;
                    case 3: MenuCategorias(); break;
                    case 4: MostrarBalance(); break;
                    case 0: salir = Entrada.Confirmar("¿Seguro que desea salir? (s/n): "); break;
                    default: Console.WriteLine("Opción no válida."); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inesperado: " + ex.Message);
                Console.WriteLine("El programa continuará ejecutándose.");
            }
        }

        Console.WriteLine("¡Hasta luego!");
    }

    private void MostrarMenuCrud(string titulo)
    {
        Console.WriteLine("\n--- " + titulo + " ---");
        Console.WriteLine("1. Agregar");
        Console.WriteLine("2. Listar");
        Console.WriteLine("3. Buscar");
        Console.WriteLine("4. Modificar");
        Console.WriteLine("5. Eliminar");
        Console.WriteLine("0. Volver");
    }

    // ===================== GASTOS =====================
    private void MenuGastos()
    {
        bool volver = false;

        while (!volver)
        {
            try
            {
                MostrarMenuCrud("GASTOS");

                switch (Entrada.LeerEntero("Opción: "))
                {
                    case 1: AgregarGasto(); break;
                    case 2: ListarGastos(); break;
                    case 3: BuscarGastos(); break;
                    case 4: ModificarGasto(); break;
                    case 5: EliminarGasto(); break;
                    case 0: volver = true; break;
                    default: Console.WriteLine("Opción no válida."); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al trabajar con los gastos: " + ex.Message);
            }
        }
    }

    private void AgregarGasto()
    {
        if (registro.Categorias.Count == 0)
        {
            Console.WriteLine("Primero debe crear al menos una categoría.");
            return;
        }

        ListarCategorias();
        int idCategoria = Entrada.LeerEntero("Número de la categoría: ");

        if (!registro.Categorias.ContainsKey(idCategoria))
        {
            Console.WriteLine("Esa categoría no existe.");
            return;
        }

        string descripcion = Entrada.LeerTexto("Descripción: ");
        decimal monto = Entrada.LeerMonto("Monto: ");
        DateTime fecha = Entrada.LeerFecha("Fecha (dd/mm/aaaa): ");

        registro.Gastos.Add(new Gasto(descripcion, monto, registro.Categorias[idCategoria], fecha));

        Console.WriteLine("Gasto agregado.");
    }

    private void MostrarGasto(int i)
    {
        Console.WriteLine((i + 1) + ". " + registro.Gastos[i]);
    }

    private void ListarGastos()
    {
        if (registro.Gastos.Count == 0)
        {
            Console.WriteLine("No hay gastos registrados.");
            return;
        }

        Console.WriteLine("\n# | Fecha | Descripción | Monto | Categoría");

        for (int i = 0; i < registro.Gastos.Count; i++)
        {
            MostrarGasto(i);
        }
    }

    private void BuscarGastos()
    {
        Console.WriteLine("Buscar por: 1. Descripción   2. Categoría   3. Rango de monto   4. Fecha");
        int opcion = Entrada.LeerEntero("Opción: ");
        int encontrados = 0;

        if (opcion == 1)
        {
            string texto = Entrada.LeerTexto("Texto a buscar: ").ToLower();

            for (int i = 0; i < registro.Gastos.Count; i++)
            {
                if (registro.Gastos[i].CoincideDescripcion(texto))
                {
                    MostrarGasto(i);
                    encontrados++;
                }
            }
        }
        else if (opcion == 2)
        {
            ListarCategorias();
            int idCategoria = Entrada.LeerEntero("Número de la categoría: ");

            if (!registro.Categorias.ContainsKey(idCategoria))
            {
                Console.WriteLine("Esa categoría no existe.");
                return;
            }

            for (int i = 0; i < registro.Gastos.Count; i++)
            {
                if (registro.Gastos[i].EsDeCategoria(idCategoria))
                {
                    MostrarGasto(i);
                    encontrados++;
                }
            }
        }
        else if (opcion == 3)
        {
            decimal minimo = Entrada.LeerMonto("Monto mínimo: ");
            decimal maximo = Entrada.LeerMonto("Monto máximo: ");

            if (minimo > maximo)
            {
                Console.WriteLine("El monto mínimo no puede ser mayor que el monto máximo.");
                return;
            }

            for (int i = 0; i < registro.Gastos.Count; i++)
            {
                if (registro.Gastos[i].MontoEnRango(minimo, maximo))
                {
                    MostrarGasto(i);
                    encontrados++;
                }
            }
        }
        else if (opcion == 4)
        {
            DateTime fecha = Entrada.LeerFecha("Fecha (dd/mm/aaaa): ");

            for (int i = 0; i < registro.Gastos.Count; i++)
            {
                if (registro.Gastos[i].EsDeFecha(fecha))
                {
                    MostrarGasto(i);
                    encontrados++;
                }
            }
        }
        else
        {
            Console.WriteLine("Opción no válida.");
            return;
        }

        if (encontrados == 0)
        {
            Console.WriteLine("No se encontraron gastos.");
        }
    }

    private void ModificarGasto()
    {
        if (registro.Gastos.Count == 0)
        {
            Console.WriteLine("No hay gastos para modificar.");
            return;
        }

        ListarGastos();
        int i = Entrada.LeerEntero("Número del gasto a modificar: ") - 1;

        if (i < 0 || i >= registro.Gastos.Count)
        {
            Console.WriteLine("Ese gasto no existe.");
            return;
        }

        Gasto gasto = registro.Gastos[i];

        Console.WriteLine("(Deje vacío y presione Enter para mantener el valor actual)");

        Console.Write("Descripción [" + gasto.Descripcion + "]: ");
        string descripcion = Entrada.LeerLinea().Trim();

        if (descripcion != "")
        {
            gasto.Descripcion = descripcion;
        }

        Console.Write("Monto [" + gasto.Monto.ToString("N2") + "]: ");
        string montoTexto = Entrada.LeerLinea().Trim();

        if (montoTexto != "")
        {
            decimal nuevoMonto;

            if (Entrada.TryParseMonto(montoTexto, out nuevoMonto))
            {
                gasto.Monto = nuevoMonto;
            }
            else
            {
                Console.WriteLine("Monto inválido, se mantiene el anterior.");
            }
        }

        Console.Write("Número de categoría [" + gasto.Categoria.Id + " - "
            + gasto.Categoria.Nombre + "]: ");
        string categoriaTexto = Entrada.LeerLinea().Trim();

        if (categoriaTexto != "")
        {
            int nuevaCategoria;

            if (int.TryParse(categoriaTexto, out nuevaCategoria)
                && registro.Categorias.ContainsKey(nuevaCategoria))
            {
                gasto.Categoria = registro.Categorias[nuevaCategoria];
            }
            else
            {
                Console.WriteLine("Categoría inválida, se mantiene la anterior.");
            }
        }

        Console.Write("Fecha [" + gasto.Fecha.ToString("dd/MM/yyyy") + "]: ");
        string fechaTexto = Entrada.LeerLinea().Trim();

        if (fechaTexto != "")
        {
            DateTime nuevaFecha;

            if (DateTime.TryParseExact(fechaTexto, "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out nuevaFecha))
            {
                gasto.Fecha = nuevaFecha.Date;
            }
            else
            {
                Console.WriteLine("Fecha inválida, se mantiene la anterior.");
            }
        }

        Console.WriteLine("Gasto modificado.");
    }

    private void EliminarGasto()
    {
        if (registro.Gastos.Count == 0)
        {
            Console.WriteLine("No hay gastos para eliminar.");
            return;
        }

        ListarGastos();
        int i = Entrada.LeerEntero("Número del gasto a eliminar: ") - 1;

        if (i < 0 || i >= registro.Gastos.Count)
        {
            Console.WriteLine("Ese gasto no existe.");
            return;
        }

        if (Entrada.Confirmar("¿Está seguro de eliminarlo? (s/n): "))
        {
            registro.Gastos.RemoveAt(i);
            Console.WriteLine("Gasto eliminado.");
        }
    }

    // ===================== INGRESOS =====================
    private void MenuIngresos()
    {
        bool volver = false;

        while (!volver)
        {
            try
            {
                MostrarMenuCrud("INGRESOS");

                switch (Entrada.LeerEntero("Opción: "))
                {
                    case 1: AgregarIngreso(); break;
                    case 2: ListarIngresos(); break;
                    case 3: BuscarIngresos(); break;
                    case 4: ModificarIngreso(); break;
                    case 5: EliminarIngreso(); break;
                    case 0: volver = true; break;
                    default: Console.WriteLine("Opción no válida."); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al trabajar con los ingresos: " + ex.Message);
            }
        }
    }

    private void AgregarIngreso()
    {
        string descripcion = Entrada.LeerTexto("Descripción: ");
        decimal monto = Entrada.LeerMonto("Monto: ");
        DateTime fecha = Entrada.LeerFecha("Fecha (dd/mm/aaaa): ");

        registro.Ingresos.Add(new Ingreso(descripcion, monto, fecha));
        Console.WriteLine("Ingreso agregado.");
    }

    private void MostrarIngreso(int i)
    {
        Console.WriteLine((i + 1) + ". " + registro.Ingresos[i]);
    }

    private void ListarIngresos()
    {
        if (registro.Ingresos.Count == 0)
        {
            Console.WriteLine("No hay ingresos registrados.");
            return;
        }

        Console.WriteLine("\n# | Fecha | Descripción | Monto");

        for (int i = 0; i < registro.Ingresos.Count; i++)
        {
            MostrarIngreso(i);
        }
    }

    private void BuscarIngresos()
    {
        Console.WriteLine("Buscar por: 1. Descripción   2. Rango de monto   3. Fecha");
        int opcion = Entrada.LeerEntero("Opción: ");
        int encontrados = 0;

        if (opcion == 1)
        {
            string texto = Entrada.LeerTexto("Texto a buscar: ").ToLower();

            for (int i = 0; i < registro.Ingresos.Count; i++)
            {
                if (registro.Ingresos[i].CoincideDescripcion(texto))
                {
                    MostrarIngreso(i);
                    encontrados++;
                }
            }
        }
        else if (opcion == 2)
        {
            decimal minimo = Entrada.LeerMonto("Monto mínimo: ");
            decimal maximo = Entrada.LeerMonto("Monto máximo: ");

            if (minimo > maximo)
            {
                Console.WriteLine("El monto mínimo no puede ser mayor que el monto máximo.");
                return;
            }

            for (int i = 0; i < registro.Ingresos.Count; i++)
            {
                if (registro.Ingresos[i].MontoEnRango(minimo, maximo))
                {
                    MostrarIngreso(i);
                    encontrados++;
                }
            }
        }
        else if (opcion == 3)
        {
            DateTime fecha = Entrada.LeerFecha("Fecha (dd/mm/aaaa): ");

            for (int i = 0; i < registro.Ingresos.Count; i++)
            {
                if (registro.Ingresos[i].EsDeFecha(fecha))
                {
                    MostrarIngreso(i);
                    encontrados++;
                }
            }
        }
        else
        {
            Console.WriteLine("Opción no válida.");
            return;
        }

        if (encontrados == 0)
        {
            Console.WriteLine("No se encontraron ingresos.");
        }
    }

    private void ModificarIngreso()
    {
        if (registro.Ingresos.Count == 0)
        {
            Console.WriteLine("No hay ingresos para modificar.");
            return;
        }

        ListarIngresos();
        int i = Entrada.LeerEntero("Número del ingreso a modificar: ") - 1;

        if (i < 0 || i >= registro.Ingresos.Count)
        {
            Console.WriteLine("Ese ingreso no existe.");
            return;
        }

        Ingreso ingreso = registro.Ingresos[i];

        Console.WriteLine("(Deje vacío y presione Enter para mantener el valor actual)");

        Console.Write("Descripción [" + ingreso.Descripcion + "]: ");
        string descripcion = Entrada.LeerLinea().Trim();

        if (descripcion != "")
        {
            ingreso.Descripcion = descripcion;
        }

        Console.Write("Monto [" + ingreso.Monto.ToString("N2") + "]: ");
        string montoTexto = Entrada.LeerLinea().Trim();

        if (montoTexto != "")
        {
            decimal nuevoMonto;

            if (Entrada.TryParseMonto(montoTexto, out nuevoMonto))
            {
                ingreso.Monto = nuevoMonto;
            }
            else
            {
                Console.WriteLine("Monto inválido, se mantiene el anterior.");
            }
        }

        Console.Write("Fecha [" + ingreso.Fecha.ToString("dd/MM/yyyy") + "]: ");
        string fechaTexto = Entrada.LeerLinea().Trim();

        if (fechaTexto != "")
        {
            DateTime nuevaFecha;

            if (DateTime.TryParseExact(fechaTexto, "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out nuevaFecha))
            {
                ingreso.Fecha = nuevaFecha.Date;
            }
            else
            {
                Console.WriteLine("Fecha inválida, se mantiene la anterior.");
            }
        }

        Console.WriteLine("Ingreso modificado.");
    }

    private void EliminarIngreso()
    {
        if (registro.Ingresos.Count == 0)
        {
            Console.WriteLine("No hay ingresos para eliminar.");
            return;
        }

        ListarIngresos();
        int i = Entrada.LeerEntero("Número del ingreso a eliminar: ") - 1;

        if (i < 0 || i >= registro.Ingresos.Count)
        {
            Console.WriteLine("Ese ingreso no existe.");
            return;
        }

        if (Entrada.Confirmar("¿Está seguro de eliminarlo? (s/n): "))
        {
            registro.Ingresos.RemoveAt(i);
            Console.WriteLine("Ingreso eliminado.");
        }
    }

    // ===================== CATEGORÍAS =====================
    private void MenuCategorias()
    {
        bool volver = false;

        while (!volver)
        {
            try
            {
                MostrarMenuCrud("CATEGORÍAS");

                switch (Entrada.LeerEntero("Opción: "))
                {
                    case 1: AgregarCategoria(); break;
                    case 2: ListarCategorias(); break;
                    case 3: BuscarCategorias(); break;
                    case 4: ModificarCategoria(); break;
                    case 5: EliminarCategoria(); break;
                    case 0: volver = true; break;
                    default: Console.WriteLine("Opción no válida."); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al trabajar con las categorías: " + ex.Message);
            }
        }
    }

    private void AgregarCategoria()
    {
        string nombre = Entrada.LeerTexto("Nombre de la categoría: ");

        if (registro.ExisteNombreCategoria(nombre, 0))
        {
            Console.WriteLine("Ya existe una categoría con ese nombre.");
            return;
        }

        registro.CrearCategoria(nombre);
        Console.WriteLine("Categoría agregada.");
    }

    private void ListarCategorias()
    {
        if (registro.Categorias.Count == 0)
        {
            Console.WriteLine("No hay categorías registradas.");
            return;
        }

        Console.WriteLine("\nCategorías:");

        foreach (Categoria categoria in registro.Categorias.Values)
        {
            Console.WriteLine(categoria);
        }
    }

    private void BuscarCategorias()
    {
        string texto = Entrada.LeerTexto("Texto a buscar: ").ToLower();
        int encontradas = 0;

        foreach (Categoria categoria in registro.Categorias.Values)
        {
            if (categoria.Nombre.ToLower().Contains(texto))
            {
                Console.WriteLine(categoria);
                encontradas++;
            }
        }

        if (encontradas == 0)
        {
            Console.WriteLine("No se encontraron categorías.");
        }
    }

    private void ModificarCategoria()
    {
        if (registro.Categorias.Count == 0)
        {
            Console.WriteLine("No hay categorías para modificar.");
            return;
        }

        ListarCategorias();
        int numero = Entrada.LeerEntero("Número de la categoría a modificar: ");

        if (!registro.Categorias.ContainsKey(numero))
        {
            Console.WriteLine("Esa categoría no existe.");
            return;
        }

        string nuevoNombre = Entrada.LeerTexto("Nuevo nombre: ");

        if (registro.ExisteNombreCategoria(nuevoNombre, numero))
        {
            Console.WriteLine("Ya existe otra categoría con ese nombre.");
            return;
        }

        registro.Categorias[numero].Nombre = nuevoNombre;
        Console.WriteLine("Categoría modificada.");
    }

    private void EliminarCategoria()
    {
        if (registro.Categorias.Count == 0)
        {
            Console.WriteLine("No hay categorías para eliminar.");
            return;
        }

        ListarCategorias();
        int numero = Entrada.LeerEntero("Número de la categoría a eliminar: ");

        if (!registro.Categorias.ContainsKey(numero))
        {
            Console.WriteLine("Esa categoría no existe.");
            return;
        }

        if (registro.CategoriaEnUso(numero))
        {
            Console.WriteLine("No se puede eliminar: hay gastos que usan esta categoría.");
            return;
        }

        if (Entrada.Confirmar("¿Está seguro de eliminarla? (s/n): "))
        {
            registro.Categorias.Remove(numero);
            Console.WriteLine("Categoría eliminada.");
        }
    }

    // ===================== BALANCE =====================
    private void MostrarBalance()
    {
        decimal totalIngresos = registro.TotalIngresos();
        decimal totalGastos = registro.TotalGastos();
        decimal balance = totalIngresos - totalGastos;

        Console.WriteLine("\n--- BALANCE ---");
        Console.WriteLine("Total ingresos: " + totalIngresos.ToString("N2"));
        Console.WriteLine("Total gastos:   " + totalGastos.ToString("N2"));
        Console.WriteLine("Balance:        " + balance.ToString("N2"));

        if (balance < 0)
        {
            Console.WriteLine("Atención: está gastando más de lo que ingresa.");
        }

        if (registro.Gastos.Count > 0)
        {
            Console.WriteLine("\nGastos por categoría:");

            foreach (Categoria categoria in registro.Categorias.Values)
            {
                decimal subtotal = registro.SubtotalCategoria(categoria.Id);

                if (subtotal > 0)
                {
                    Console.WriteLine("   " + categoria.Nombre + ": " + subtotal.ToString("N2"));
                }
            }
        }
    }
}

class Program
{
    static void Main()
    {
        Aplicacion app = new Aplicacion();
        app.Ejecutar();
    }
}