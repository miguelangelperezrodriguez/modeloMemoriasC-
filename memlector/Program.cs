using System;
using System.Collections;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LectorMemorias
{
    internal class Program
    {
        public struct peticion_ejecucion
        {
            public int idx_mapa;
            public int tam_dato;
            public int max_nro_datos;
            public bool b_valor_defecto;
            public char caracter_defecto;
        }

        private const int NRO_VARIABLES_ARRAYS = 25;

        private static MemoryMappedFile mmap_nucleo;

        private static MemoryMappedFile mmap_tabla_ejecucion;

        private static MemoryMappedFile mmap_1;
        private static MemoryMappedFile mmap_2;
        private static MemoryMappedFile mmap_3;

        private static MemoryMappedFile[] mmap_ejecucion = new MemoryMappedFile[NRO_VARIABLES_ARRAYS];

        private static Dictionary<string, peticion_ejecucion> tablaPeticiones = new Dictionary<string, peticion_ejecucion>();

        private Mutex mutex_nucleo;

        static void Main(string[] args)
        {

            int tam_zona1 = 2000;
            int tam_zona2 = 2000;
            int tam_zona3 = 2000;
            int tam_zona4 = 2000;
            int tam_zona_dim1 = 2000;
            int tam_zona_dim2 = 2000;
            int tam_zona_dim3 = 2000;
            int tam_zona_dim4 = 2000;
            int tam_din_dato;


            try
            {
                mmap_nucleo = MemoryMappedFile.OpenExisting("nucleo");
                var access = mmap_nucleo.CreateViewAccessor(0, sizeof(int)*9);
                access.Read(0, out tam_zona1);
                access.Read(sizeof(int),    out tam_zona2);
                access.Read(sizeof(int)*2,  out tam_zona3);
                access.Read(sizeof(int)*3,  out tam_zona4);
                access.Read(sizeof(int)*4,  out tam_zona_dim1);
                access.Read(sizeof(int)*5,  out tam_zona_dim2);
                access.Read(sizeof(int)*6,  out tam_zona_dim3);
                access.Read(sizeof(int)*7,  out tam_zona_dim4);
                access.Read(sizeof(int)*8,  out tam_din_dato);
                access.Dispose();
                mmap_nucleo.Dispose();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en los datos.");
                return;
            }

            Console.WriteLine("Leyendo memoria...");

            int tam_zonas = tam_zona1 + tam_zona2 + tam_zona3 + tam_zona4;
            string content_mapa1, content_mapa2, content_mapa3, content_mapa4;
            string[] datos_estatica_array;
            string[] datos_temprano_array;
            List<string> datos_estatica_lista;
            List<string> datos_temprano_lista;

            // ESTATICA

            mmap_1 = MemoryMappedFile.OpenExisting("mapa_1");
            var accessor = mmap_1.CreateViewAccessor(0,tam_zonas);


            int tam;
            // ZONA 1. ARRAY ESTATICO
            accessor.Read(0, out tam);
            byte[] readData1 = new byte[tam];
            accessor.ReadArray(sizeof(int), readData1, 0, readData1.Length);
            content_mapa1 = Encoding.UTF8.GetString(readData1);
            Console.WriteLine($"Leído de vuelta: {content_mapa1}");

            datos_estatica_array = content_mapa1.Split("|");


            readData1 = null;

            // ZONA2. LISTA ESTATICA
            accessor.Read(tam_zona1, out tam);
            byte[] readData2 = new byte[tam];
            accessor.ReadArray(tam_zona1+sizeof(int), readData2, 0, readData2.Length);
            content_mapa2 = Encoding.UTF8.GetString(readData2);
            Console.WriteLine($"Leído de vuelta: {content_mapa2}");


            datos_estatica_lista = content_mapa2.Split("|").ToList();

            readData2 = null;


            // ZONA 3. ARRAY TEMPRANO
            accessor.Read(tam_zona1+tam_zona2, out tam);
            byte[] readData3 = new byte[tam];
            accessor.ReadArray(tam_zona1+tam_zona2+sizeof(int), readData3, 0, readData3.Length);
            content_mapa3 = Encoding.UTF8.GetString(readData3);
            Console.WriteLine($"Leído de vuelta: {content_mapa3}");

            datos_temprano_array = content_mapa3.Split("|");

            readData3 = null;

            // ZONA 4. LISTA TEMPRANA
            accessor.Read(tam_zona1 + tam_zona2 + tam_zona3, out tam);
            byte[] readData4 = new byte[tam];
            accessor.ReadArray(tam_zona1 + tam_zona2 + tam_zona3 + sizeof(int),
                readData4, 0, readData4.Length);
            content_mapa4 = Encoding.UTF8.GetString(readData4);
            Console.WriteLine($"Leído de vuelta: {content_mapa4}");

            datos_temprano_lista  =  content_mapa4.Split("|").ToList();

            readData4 = null;


            accessor.Dispose();
            mmap_1.Dispose();

            // DINAMICA
            int tam_zonas_din = tam_zona_dim1 + tam_zona_dim2 + tam_zona_dim3 + tam_zona_dim4;
            // tam_din_dato



            string[] datos_estatica_din_array = new string[tam_zona_dim1];
            string[] datos_temprano_din_array_POO = new string[tam_zona_dim2];
            List<string> datos_estatica_din_lista;
            List<string> datos_temprano_din_lista_POO;

            mmap_2 = MemoryMappedFile.OpenExisting("mapa_2");
            var accessor_din = mmap_2.CreateViewAccessor(0, tam_zonas_din);

            // ejemplo 1
            // Leer objeto 3 de sector 1
            int posicion = 0 + (3 * tam_din_dato);
            //view_dinamico.ReadArray<byte>(desplazamiento, dato_leido, 0, torre_inicio_dinamica.tam_dato);
            byte[] dato_leido = new byte[tam_din_dato];
            accessor_din.ReadArray<byte>(posicion, dato_leido, 0, tam_din_dato);
            Console.Write($"Dinamica : ");
            Console.WriteLine(dato_leido[0].ToString());


            // ejemplo 2
            // Leer objeto 
            posicion = tam_zona_dim1 + (2 * tam_din_dato);
            dato_leido = new byte[tam_din_dato];
            accessor_din.ReadArray<byte>(posicion, dato_leido, 0, tam_din_dato);
            Console.Write($"Dinamica defecto : ");
            Console.WriteLine(dato_leido[0].ToString());



            // EJECUCION
            int nro_peticiones;
            int tam_linea = 100 + sizeof(int) + sizeof(int);
            byte[] peticion_leida = new byte[tam_linea];
            mmap_tabla_ejecucion = MemoryMappedFile.OpenExisting("tabla");
            var accessor_ejecucion_nro_datos = mmap_tabla_ejecucion.CreateViewAccessor(0,sizeof(int));
            accessor_ejecucion_nro_datos.Read(0, out nro_peticiones);
            accessor_ejecucion_nro_datos.Dispose();

            List<byte[]> l_peticiones = new List<byte[]>();

            var accessor_ejecucion_tabla = mmap_tabla_ejecucion.CreateViewAccessor(sizeof(int), nro_peticiones * tam_linea);

            int offset = 100 + sizeof(int) + sizeof(int);
            for (int i=1;i<=nro_peticiones;i++)
            {
                byte[] destino = new byte[offset];
                accessor_ejecucion_tabla.ReadArray<byte>(offset*(i-1), peticion_leida, 0, tam_linea);
                peticion_leida.CopyTo(destino, 0); 
                l_peticiones.Add(destino);
            }

            accessor_ejecucion_tabla.Dispose();

            // Abrir los MMF segun la tabla.

            int idx_mapa = 0;
            foreach (byte[] pet in l_peticiones)
            {
                byte[] pet1 = new byte[100];
                Array.Copy(pet, 0, pet1, 0, 100);

                string pet2 = Encoding.UTF8.GetString(pet1).Trim();

                byte[] tam_peticion = pet[100..104];
                byte[] max = pet[104..108];

                int i_tam_peticion = BitConverter.ToInt32(tam_peticion, 0);
                int i_max = BitConverter.ToInt32(max, 0);

                // Abrimos el mapa de datos.
                mmap_ejecucion[idx_mapa] = MemoryMappedFile.OpenExisting(pet2);



                idx_mapa++;

            }

            // Lectura de un valor.
            // P.ej :
            // idx_mapa : 0 . var1
            // i_tam_peticion : 100
            // i_max : 100
            // posicion_a_leer : 3

            // Buscar
            // var1     y leer posicion 3 
            string mapa_pedido = "var1";
            idx_mapa = 0;
            int posicion_ej = 3;
            byte[] resultado;


            Console.WriteLine("Escriba nombre mapa :");
            mapa_pedido = Console.ReadLine();
            Console.WriteLine("Posición del dato : ");
            string s = Console.ReadLine();
            posicion_ej = Int32.Parse(s);

            foreach (byte[] pet in l_peticiones)
            {
                byte[] pet1 = new byte[100];
                Array.Copy(pet, 0, pet1, 0, 100);

                string pet2 = Encoding.UTF8.GetString(pet1).Trim();

                if (pet2 == mapa_pedido)
                {

                    byte[] tam_peticion = pet[100..104];
                    byte[] max = pet[104..108];

                    int i_tam_peticion = BitConverter.ToInt32(tam_peticion, 0);
                    int i_max = BitConverter.ToInt32(max, 0);

                    var accessor_ej = mmap_ejecucion[idx_mapa].CreateViewAccessor(i_tam_peticion*(posicion_ej-1),
                        i_tam_peticion);

                    resultado = new byte[i_tam_peticion];
                    accessor_ej.ReadArray(0, resultado, 0, i_tam_peticion);

                    Console.WriteLine(resultado[0].ToString());

                    accessor_ej.Dispose();

                    break;
                }

                idx_mapa++;

            }


            Console.ReadKey();

            accessor.Dispose();
            accessor_din.Dispose();
            mmap_1.Dispose();
            mmap_2.Dispose();
            mmap_nucleo.Dispose();
            mmap_tabla_ejecucion.Dispose();

            mmap_tabla_ejecucion.Dispose();


            Console.ReadKey();

        }
    }
}
