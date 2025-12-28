using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemoriaSeparados;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;

namespace MemoriaTorre
{
    public struct t_variable
    {
        public string nombre_mapa;
        public int idx_dato;
        public byte[] valor;
    }
    public class MemoriaTorre
    {
        private const string clave_mutex_nucleo =   "Nucleo";
        private const string clave_mutex_estatica = "Estatica";
        private const string clave_mutex_dinamica = "Dinamica";

        private const int NRO_VARIABLES_ARRAYS = 25;

        Mutex mutex_nucleo;
        Mutex mutex_estatica;
        Mutex mutex_dinamica;

        InicioEstatica torre_inicio_estatica;
        InicioDinamica torre_inicio_dinamica;
        //Ejecucion torre_ejecucion;

        MemoryMappedFile mmap_inicio;

        MemoryMappedFile mmap_inicio_dinamico;
        MemoryMappedViewAccessor view_dinamico;

        MemoryMappedFile mmap_nucleo;
        MemoryMappedViewAccessor view_nucleo;

        MemoryMappedFile mmap_tabla_ejecucion;
        MemoryMappedViewAccessor view_ejecucion;

        // DATOS TIPO ARRAYS Y VARIABLES.

        Ejecucion torre_ejecucion;
        MemoryMappedFile[] mmap_tabla_variablesarray = new MemoryMappedFile[NRO_VARIABLES_ARRAYS];
        MemoryMappedViewAccessor view_tabla;

        // Tamaños leidos
        int tam_1_leido,tam_2_leido, tam_3_leido,tam_4_leido,
            tam_1_leido_dim,tam_2_leido_dim,tam_3_leido_dim, tam_4_leido_dim,
            tam_dato_leido;

        // Variables
        Dictionary<string, t_variable> dicc_variables;

        public MemoriaTorre()
        {
            torre_inicio_estatica = new InicioEstatica();
            torre_ejecucion = new Ejecucion();
        }

        public void DestruirMapas()
        {
            mmap_inicio.Dispose();
            mmap_inicio_dinamico.Dispose();
            mmap_nucleo.Dispose();
            mmap_tabla_ejecucion.Dispose();

            int i = 0;
            foreach (var peticion in torre_ejecucion.tablaPeticiones)
            {
                mmap_tabla_variablesarray[i].Dispose();
                i++;
            } 
        }

        public bool CompartirTamanyos (int tam_1,int tam_2,int tam_3,int tam_4,
            int tam_dim_1,int tam_dim_2, int tam_dim_3, int tam_dim_4,
            int tam_dato)
        {
            tam_1_leido = tam_1;
            tam_2_leido = tam_2;
            tam_3_leido = tam_3;
            tam_4_leido = tam_4;
            tam_1_leido_dim = tam_dim_1;
            tam_2_leido_dim = tam_dim_2;
            tam_3_leido_dim = tam_dim_3;
            tam_4_leido_dim = tam_dim_4;
            tam_dato_leido = tam_dato;

            bool nueva;

            try
            {
                mutex_nucleo = new Mutex(true, clave_mutex_nucleo,out nueva);

                mutex_nucleo.WaitOne();

                mmap_nucleo = MemoryMappedFile.CreateNew("nucleo",sizeof(int)*9);
                view_nucleo = mmap_nucleo.CreateViewAccessor(0, sizeof(int) * 9 );

                view_nucleo.Write(0,tam_1_leido);
                view_nucleo.Write(sizeof(int),tam_2_leido);
                view_nucleo.Write(sizeof(int)*2, tam_3_leido);
                view_nucleo.Write(sizeof(int)*3, tam_4_leido);
                view_nucleo.Write(sizeof(int)*4, tam_1_leido_dim);
                view_nucleo.Write(sizeof(int)*5, tam_2_leido_dim);
                view_nucleo.Write(sizeof(int)*6, tam_3_leido_dim);
                view_nucleo.Write(sizeof(int)*7, tam_4_leido_dim);
                view_nucleo.Write(sizeof(int) * 8, tam_dato_leido);

                mutex_nucleo.ReleaseMutex();


                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al hacer núcleo.");
                return false;
            }

        }

        // ESTATICAS

        public void DefinirInicioEstatica(bool bvalorDefecto,
            string valorDefecto,
            int tam_estatico_1 = 0, int tam_estatico_2 = 0)
        {
            torre_inicio_estatica.IniciarEstatica(bvalorDefecto, valorDefecto,
                tam_estatico_1, tam_estatico_2,
                tam_1_leido, tam_2_leido, tam_3_leido, tam_4_leido);
        }

        public void DefinirTempranoEstatica(bool bvalorDefecto, string valorDefecto,
            int tam_estatico_3, int tam_estatico_4)
        {
            torre_inicio_estatica.IniciarDatosTempranosEstatica(bvalorDefecto, valorDefecto,
                tam_estatico_3, tam_estatico_4);
        }

        // DINAMICAS
        public void DefinirInicioDinamica(int tam_dato)
        {
            torre_inicio_dinamica = new InicioDinamica(tam_dato,
                tam_1_leido_dim, tam_2_leido_dim, tam_3_leido_dim, tam_4_leido_dim);
        }


        // CREA UN MAPA DE FICHERO EN MEMORIA.
        public bool CompartirInicioEstaticaInicio(string clave_mapa,
            bool b_dejado_en_espera)
        {
            // Creamos todos los objetos de inicio con los tamaños.
            bool nueva;
            mutex_estatica = new Mutex(true, clave_mutex_estatica, out nueva);
            try
            {
                

                int tam_total = torre_inicio_estatica.tam_mapa1 +
                    torre_inicio_estatica.tam_mapa2 + torre_inicio_estatica.tam_mapa3 +
                    torre_inicio_estatica.tam_mapa4;

                // Semaforo al acceder al mapa
                mutex_estatica.WaitOne();


                mmap_inicio = MemoryMappedFile.CreateNew(clave_mapa, tam_total);
                var accessor = mmap_inicio.CreateViewAccessor(0, tam_total);

                string textoConcatenado1 = string.Join("|", InicioEstatica.cadenasNombresIniEstatica);
                byte[] bytesUtf8 = Encoding.UTF8.GetBytes(textoConcatenado1);
                // El array lo guarda en el primer sector de memoria.
                accessor.Write(0, bytesUtf8.Length);
                accessor.WriteArray(sizeof(int), bytesUtf8, 0, bytesUtf8.Length);

                Console.WriteLine($"DATOS ARRAY INICIO ESTATICO ENVIADOS : {bytesUtf8.Length.ToString()}");

                // La lista se guarda en el segundo sector
                StringBuilder textoConcatenado2 = new StringBuilder();
                foreach (string dato in InicioEstatica.listaNombresIniConCapacidad)
                {
                    textoConcatenado2.Append(dato);
                    textoConcatenado2.Append("|");
                }
                byte[] bytesUtf8_1 = Encoding.UTF8.GetBytes(textoConcatenado2.ToString());
                // Se guarda en el segundo sector de memoria.

                int desp_lista = torre_inicio_estatica.tam_mapa1;

                accessor.Write(desp_lista, bytesUtf8_1.Length);
                accessor.WriteArray(desp_lista + sizeof(int), bytesUtf8_1, 0, bytesUtf8_1.Length);

                Console.WriteLine($"DATOS LISTA INICIO ESTATICO ENVIADOS : {bytesUtf8_1.Length.ToString()}");

                mutex_estatica.ReleaseMutex();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en escritor estático inicial : {ex.Message}");
            }

            if (b_dejado_en_espera)
            {
                while (true) { }
            }

            return true;
        }

        public bool CompartirInicioEstaticaTemprana(string clave_mapa, long tam_array, long tam_lista,
            bool b_dejado_en_espera)
        {
            bool nueva;

            int tam_total = torre_inicio_estatica.tam_mapa1 +
                torre_inicio_estatica.tam_mapa2 + torre_inicio_estatica.tam_mapa3 +
                torre_inicio_estatica.tam_mapa4;

            mutex_estatica = new Mutex(true, clave_mutex_estatica, out nueva);

            try
            {

                mutex_estatica.WaitOne();

                var accessor = mmap_inicio.CreateViewAccessor(0, tam_total);

                int desp1 = torre_inicio_estatica.tam_mapa1 + torre_inicio_estatica.tam_mapa2;
                int desp2 = torre_inicio_estatica.tam_mapa1 + torre_inicio_estatica.tam_mapa2 +
                    torre_inicio_estatica.tam_mapa3;

                string textoConcatenado1 = string.Join("|", torre_inicio_estatica.cadenasNombresIniEstaticaPOO);
                byte[] bytesUtf8 = Encoding.UTF8.GetBytes(textoConcatenado1);
                // El array lo guarda en el primer sector de memoria.
                accessor.Write(desp1, bytesUtf8.Length);
                accessor.WriteArray(desp1 + sizeof(int), bytesUtf8, 0, bytesUtf8.Length);

                Console.WriteLine($"DATOS ARRAY INICIO TEMPRANOS ENVIADOS : {bytesUtf8.Length.ToString()}");

                StringBuilder textoConcatenado2 = new StringBuilder();
                foreach (string dato in torre_inicio_estatica.listaNombresIniConCapacidadPOO)
                {
                    textoConcatenado2.Append(dato);
                    textoConcatenado2.Append("|");
                }
                byte[] bytesUtf8_1 = Encoding.UTF8.GetBytes(textoConcatenado2.ToString());
                // Se guarda en el segundo sector de memoria.

                accessor.Write(desp2, bytesUtf8_1.Length);
                accessor.WriteArray(desp2 + sizeof(int), bytesUtf8_1, 0, bytesUtf8_1.Length);

                Console.WriteLine($"DATOS LISTA INICIO TEMPRANOS ENVIADOS : {bytesUtf8_1.Length.ToString()}");

                mutex_estatica.ReleaseMutex();


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en escritor estático temprano : {ex.Message}");
            }

            if (b_dejado_en_espera)
            {
                while (true) { }
            }

            return true;
        }

        public bool CompartirInicioDinamica(string clave_mapa,
            bool b_dejado_en_espera)
        {
            // Creamos todos los objetos de inicio con los tamaños.
            int tam_total = torre_inicio_dinamica.tam_dinamico1 + torre_inicio_dinamica.tam_dinamico2 +
                torre_inicio_dinamica.tam_dinamico3 + torre_inicio_dinamica.tam_dinamico4;
            try
            {
                if (mmap_inicio_dinamico==null)
                    mmap_inicio_dinamico = MemoryMappedFile.CreateNew(clave_mapa, tam_total);
                view_dinamico = mmap_inicio_dinamico.CreateViewAccessor(0, tam_total);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en escritor dinámico : {ex.Message}");
            }

            if (b_dejado_en_espera)
            {
                while (true) { }
            }
            return true;
        }

        // nro_mapa : 1-4
        public bool EscribirenInicioDinamica(int nro_mapa, int posicion,
            byte[] dato_a_escribir)
        {
            bool nueva;

            mutex_dinamica = new Mutex(true, clave_mutex_dinamica, out nueva);
            try
            {
                int desplazamiento = 0;
                switch (nro_mapa)
                {

                    case 1:
                        desplazamiento = torre_inicio_dinamica.tam_dato * posicion;
                        break;
                    case 2:
                        desplazamiento = torre_inicio_dinamica.tam_dinamico1 +
                            torre_inicio_dinamica.tam_dato * posicion;
                        break;
                    case 3:
                        desplazamiento = torre_inicio_dinamica.tam_dinamico1 +
                            torre_inicio_dinamica.tam_dinamico2 +
                            torre_inicio_dinamica.tam_dato * posicion;
                        break;
                    case 4:
                        desplazamiento = torre_inicio_dinamica.tam_dinamico1 +
                            torre_inicio_dinamica.tam_dinamico2 +
                            torre_inicio_dinamica.tam_dinamico3 +
                            torre_inicio_dinamica.tam_dato * posicion;
                        break;
                }

                mutex_dinamica.WaitOne();


                view_dinamico.WriteArray(desplazamiento, dato_a_escribir, 0, dato_a_escribir.Length);

                switch (nro_mapa)
                {

                    case 1:
                        desplazamiento = torre_inicio_dinamica.tam_dato * posicion;
                        torre_inicio_dinamica.Escribir_en_EstructuraZona1(desplazamiento, dato_a_escribir);
                        break;
                    case 2:
                        desplazamiento = torre_inicio_dinamica.tam_dinamico1 +
                            torre_inicio_dinamica.tam_dato * posicion;
                        torre_inicio_dinamica.Escribir_en_EstructuraZona2(desplazamiento, dato_a_escribir);
                        break;
                    case 3:
                        desplazamiento = torre_inicio_dinamica.tam_dinamico1 +
                            torre_inicio_dinamica.tam_dinamico2 +
                            torre_inicio_dinamica.tam_dato * posicion;
                        torre_inicio_dinamica.Escribir_en_EstructuraZona3(desplazamiento, dato_a_escribir);

                        break;
                    case 4:
                        desplazamiento = torre_inicio_dinamica.tam_dinamico1 +
                            torre_inicio_dinamica.tam_dinamico2 +
                            torre_inicio_dinamica.tam_dinamico3 +
                            torre_inicio_dinamica.tam_dato * posicion;
                        torre_inicio_dinamica.Escribir_en_EstructuraZona4(desplazamiento, dato_a_escribir);
                        break;
                }

                mutex_dinamica.ReleaseMutex();

                return true;

            }
            catch (Exception ex)
            {
                return false;
            }


        }

        public bool LeerenInicioDinamica(int nro_mapa, int posicion,
            out byte[] dato_leido)
        {
            bool nueva;

            dato_leido = new byte[torre_inicio_dinamica.tam_dato];

            mutex_dinamica = new Mutex(true, clave_mutex_dinamica, out nueva);

            try
            {
                int desplazamiento = 0;

                switch (nro_mapa)
                {

                    case 1:
                        desplazamiento = torre_inicio_dinamica.tam_dato * posicion;
                        break;
                    case 2:
                        desplazamiento = torre_inicio_dinamica.tam_dinamico1 +
                            torre_inicio_dinamica.tam_dato * posicion;
                        break;
                    case 3:
                        desplazamiento = torre_inicio_dinamica.tam_dinamico1 +
                            torre_inicio_dinamica.tam_dinamico2 +
                            torre_inicio_dinamica.tam_dato * posicion;
                        break;
                    case 4:
                        desplazamiento = torre_inicio_dinamica.tam_dinamico1 +
                            torre_inicio_dinamica.tam_dinamico2 +
                            torre_inicio_dinamica.tam_dinamico3 +
                            torre_inicio_dinamica.tam_dato * posicion;
                        break;
                }

                mutex_dinamica.WaitOne();

                view_dinamico.ReadArray<byte>(desplazamiento, dato_leido, 0, torre_inicio_dinamica.tam_dato);

                mutex_dinamica.ReleaseMutex();

                return true;
            }

            catch (Exception ex) 
            { 
                return false; 
            
            }

        }

        public void AddPeticionEjecucion (string pet_nombre, peticion_ejecucion pet)
        {
            torre_ejecucion.AddPeticion(pet_nombre,pet.idx_mapa, pet.tam_dato, pet.max_nro_datos,
                pet.b_valor_defecto, pet.caracter_defecto);

        }

        public bool CompartirEjecucion ()
        {
            try
            {
                int i = 0;
                foreach (var peticion in torre_ejecucion.tablaPeticiones)
                {
                    int capacidad = peticion.Value.tam_dato * peticion.Value.max_nro_datos;
                    mmap_tabla_variablesarray[i] = MemoryMappedFile.CreateNew(peticion.Key, capacidad);
                    i++;
                }
                // Escribir la tabla
                // El tamaño que tiene la tabla es para 100 de string y tam_dato y max_nro_datos.
                int tam_tabla = sizeof(int) + torre_ejecucion.tablaPeticiones.Count * (100 + sizeof(int) + sizeof(int));
                mmap_tabla_ejecucion = MemoryMappedFile.CreateNew("tabla", tam_tabla);
                var accessor = mmap_tabla_ejecucion.CreateViewAccessor(0, tam_tabla);

                accessor.Write(0, torre_ejecucion.tablaPeticiones.Count);

                i = 0;
                foreach (var peticion in torre_ejecucion.tablaPeticiones)
                {
                    int offset = 100 + sizeof(int) + sizeof(int);
                    string clave_rellenada = peticion.Key.PadRight(100);
                    byte[] clave_bytes = Encoding.ASCII.GetBytes(clave_rellenada);
                    byte[] byte_tam_dato = BitConverter.GetBytes(peticion.Value.tam_dato);
                    byte[] byte_max_nro_datos = BitConverter.GetBytes(peticion.Value.max_nro_datos);
                    byte[] dato_escribir = clave_bytes.Concat(byte_tam_dato).Concat(byte_max_nro_datos).ToArray();
                    accessor.WriteArray((offset * i)+sizeof(int), dato_escribir, 0, dato_escribir.Length);
                    i++;
                }

                accessor.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool EscribirEnEjecucion (string nombre_mapa, int idx_dato, byte[] dato)
        {
            try
            {
                int offset = torre_ejecucion.tablaPeticiones[nombre_mapa].tam_dato * (idx_dato - 1);
                int idx_mapa = torre_ejecucion.tablaPeticiones[nombre_mapa].idx_mapa-1;
                int tam_mapa = torre_ejecucion.tablaPeticiones[nombre_mapa].max_nro_datos *
                    torre_ejecucion.tablaPeticiones[nombre_mapa].tam_dato;

                var accessor = mmap_tabla_variablesarray[idx_mapa].CreateViewAccessor(0, tam_mapa);

                accessor.WriteArray(offset,dato,0, dato.Length);

                accessor.Dispose();
            }
            catch (Exception e)
            {
                return false;
            }

            return true;

        }

        public bool LeerEnEjecucion (string nombre_mapa, int idx_dato, out byte[] dato)
        {
            dato = null;
            try
            {
                int offset = torre_ejecucion.tablaPeticiones[nombre_mapa].tam_dato * (idx_dato - 1);
                int idx_mapa = torre_ejecucion.tablaPeticiones[nombre_mapa].idx_mapa-1;
                int tam_mapa = torre_ejecucion.tablaPeticiones[nombre_mapa].max_nro_datos *
                    torre_ejecucion.tablaPeticiones[nombre_mapa].tam_dato;

                var accessor = mmap_tabla_variablesarray[idx_mapa].CreateViewAccessor(0, tam_mapa);
                dato = new byte[torre_ejecucion.tablaPeticiones[nombre_mapa].tam_dato];
                accessor.ReadArray(offset, dato, 0, dato.Length);

                accessor.Dispose();

            }
            catch {  return false; }
            return true;
        }

        public void AddVariable (string nombre, t_variable info_variable)
        {
            dicc_variables.Add(nombre, info_variable);
        }

        public bool EscribirVariable(string nombre_var, byte[] valor)
        {
            try
            {
                string nombre_mapa = dicc_variables[nombre_var].nombre_mapa;
                int indice_dato = dicc_variables[nombre_var].idx_dato;

                EscribirEnEjecucion(nombre_mapa, indice_dato, valor);

                return true;
            }
            catch { return false; }
        }

    }
}

