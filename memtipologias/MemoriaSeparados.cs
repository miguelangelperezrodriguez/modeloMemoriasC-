using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MemoriaSeparados
{
    // INICIO. PAREJA. 

    // Se llama al iniciar el programa
    public class InicioEstatica
    {

        // Estatica y estatica para la clase
        // TIEMPO DE COMPILACION
        public static string[] cadenasNombresIniEstatica;
        // Está no pierde ser dinámica
        // Habría que filtrar que no se aumente el tamaño.
        public static List<string> listaNombresIniConCapacidad = new List<string>();

        // TIEMPO TEMPRANO DE EJECUCION
        // Estatica y dinamica para la clase.
        public string[] cadenasNombresIniEstaticaPOO;
        // Está no pierde ser dinámica
        // Habría que filtrar que no se aumente el tamaño.
        public List<string> listaNombresIniConCapacidadPOO = new List<string>();

        public int tam_estatico1 = 0, tam_estatico2 = 0, tam_estatico3 = 0, tam_estatico4 = 0;
        public int tam_mapa1, tam_mapa2, tam_mapa3, tam_mapa4 = 0;

        public  bool b_definido_inicio, b_definido_temprano = false;

        // Para arrancar clase.
        public InicioEstatica()
        { }

        // PASO 1
        public void IniciarEstatica(bool bvalorDefecto,
            string valorDefecto,
            int tam_estatico_1 = 0, int tam_estatico_2 = 0, 
            int tam_mapa_estatico_1 = 0, int tam_mapa_estatico_2 = 0, int tam_mapa_estatico_3 = 0, int tam_mapa_estatico_4 = 0)
        {

            cadenasNombresIniEstatica = new string[tam_estatico_1];
            listaNombresIniConCapacidad = new List<string>((int)tam_estatico_2);

            if (bvalorDefecto)
            {
                for (int i = 0; i < tam_estatico_1; i++)
                    cadenasNombresIniEstatica[i] = valorDefecto;
                listaNombresIniConCapacidad = Enumerable.Repeat(valorDefecto, (int)tam_estatico_2).ToList();
            }

            tam_estatico1 = tam_estatico_1;
            tam_estatico2 = tam_estatico_2;

            // Ponemos los tamaños del buffer de sus partes.
            tam_mapa1 = tam_mapa_estatico_1;
            tam_mapa2 = tam_mapa_estatico_2;
            tam_mapa3 = tam_mapa_estatico_3;
            tam_mapa4 = tam_mapa_estatico_4;

            b_definido_inicio = true;

        }

        // PASO2
        public void IniciarDatosTempranosEstatica (bool bvalorDefecto , string valorDefecto,
            int tam_estatico_3, int tam_estatico_4)
        {
            cadenasNombresIniEstaticaPOO = new string[tam_estatico_3];
            listaNombresIniConCapacidadPOO = new List<string>(tam_estatico_4);

            if (bvalorDefecto)
            {
                for (int i = 0; i < tam_estatico_3; i++)
                    cadenasNombresIniEstaticaPOO[i] = valorDefecto;
                listaNombresIniConCapacidadPOO = Enumerable.Repeat(valorDefecto, (int)tam_estatico_4).ToList();
            }

            tam_estatico3 = tam_estatico_3;
            tam_estatico4 = tam_estatico_4;

            b_definido_temprano = true;
        }

    }

    class InicioDinamica
    {
        private static byte[] cadenasNombresIniDinamica;
        private static List<byte> listaNombresIniDinamica;

        private byte[] cadenasNombresIniDinamicaPOO;
        private List<byte> listaNombresIniDinamicaPOO;

        public int tam_dato { get; }
        public int tam_dinamico1 = 0, tam_dinamico2 = 0, tam_dinamico3 = 0, tam_dinamico4 = 0;

        public InicioDinamica(int tam_dato_pasado, int tam_mapa_dinamico_1 = 0, int tam_mapa_dinamico_2 = 0,
            int tam_mapa_dinamico_3 = 0, int tam_mapa_dinamico_4 = 0)

        {
            tam_dato = tam_dato_pasado;

            tam_dinamico1 = tam_mapa_dinamico_1;
            tam_dinamico2 = tam_mapa_dinamico_2;
            tam_dinamico3 = tam_mapa_dinamico_3;
            tam_dinamico4 = tam_mapa_dinamico_4;

            cadenasNombresIniDinamica = new byte[tam_dinamico1];
            listaNombresIniDinamica = new List<byte>(tam_dinamico2);
            cadenasNombresIniDinamicaPOO = new byte[tam_dinamico3];
            listaNombresIniDinamicaPOO = new List<byte>(tam_dinamico4);

        }

        public void Escribir_en_EstructuraZona1 (int posicion, byte[] dato)
        {
            Array.Copy(dato, 0, cadenasNombresIniDinamica, posicion, tam_dato);
        }

        public void Escribir_en_EstructuraZona2 (int posicion, byte[] dato)
        {
            listaNombresIniDinamica.AddRange(dato);
        }

        public void Escribir_en_EstructuraZona3 (int posicion, byte[] dato)
        {
            Array.Copy(dato, 0, cadenasNombresIniDinamicaPOO, posicion, tam_dato);
        }

        public void Escribir_en_EstructuraZona4(int posicion, byte[] dato)
        {
            listaNombresIniDinamicaPOO.AddRange(dato);
        }

    }

    // EJECUCION. 

    // TIPO DE PETICIONES 
    public struct peticion_ejecucion
    {
        public int idx_mapa;
        public string nombre_mapa;
        public int tam_dato;
        public int max_nro_datos;
        public bool b_valor_defecto;
        public char caracter_defecto;
    }

    public class Ejecucion
    {

        public Dictionary<string,peticion_ejecucion> tablaPeticiones;
        

        public Ejecucion ()
        {
            tablaPeticiones = new Dictionary<string, peticion_ejecucion>();
        }

        public bool AddPeticion (string pet_nombre,int pet_idx,int pet_tam_dato,int pet_max_nro_datos,
            bool b_valor, char car_relleno)
        {
            peticion_ejecucion pet = new peticion_ejecucion ();
            pet.idx_mapa = pet_idx;
            pet.nombre_mapa = pet_nombre;
            pet.tam_dato = pet_tam_dato;
            pet.max_nro_datos = pet_max_nro_datos;
            pet.b_valor_defecto = b_valor;
            pet.caracter_defecto = car_relleno;

            tablaPeticiones.Add (pet_nombre, pet);

            return true;
        }

    }
}


