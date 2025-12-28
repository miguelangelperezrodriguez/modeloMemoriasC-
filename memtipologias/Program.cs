using MemoriaSeparados;
using MemoriaTorre;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace MemoriaTipologias
{
    internal class Program
    {
        static void Main(string[] args)
        {

            const long TAM_ESTATICO = 1000;
            Int32 tam_1, tam_2, tam_3, tam_4;
            Int32 uso_1, uso_2, uso_3, uso_4;

            Int32 tam_1_dim, tam_2_dim, tam_3_dim, tam_4_dim;
            Int32 tam_en_bytes_dato;


            // MemoriaTipologias.exe 2000 2000 2000 2000 100 100 100 100 100 2000 2000 2000 2000 
            if (args.Length != 13)
            {
                Console.WriteLine("MemoriaTipologias <tam1> <tam2> <tam3> <tam4> " +
                    "<usoarrayestatico> <usolistaestatico> <usoarraytemprano> <usolistatemprano> " +
                    "<tamdato> <tam1> <tam2> <tam3> <tam4>");
                return;    
            }

            tam_1 = Int32.Parse(args[0]);
            tam_2 = Int32.Parse(args[1]);
            tam_3 = Int32.Parse(args[2]);
            tam_4 = Int32.Parse(args[3]);

            uso_1 = Int32.Parse(args[4]);
            uso_2 = Int32.Parse(args[5]);
            uso_3 = Int32.Parse(args[6]);
            uso_4 = Int32.Parse(args[7]);

            tam_en_bytes_dato = Int32.Parse(args[8]);

            tam_1_dim = Int32.Parse(args[9]);
            tam_2_dim = Int32.Parse(args[10]);
            tam_3_dim = Int32.Parse(args[11]);
            tam_4_dim = Int32.Parse(args[12]);

            try
            {
                using (StreamWriter wr = new StreamWriter(args[0]))
                {
                    // ESTATICO
                    wr.WriteLine(tam_1.ToString());
                    wr.WriteLine(tam_2.ToString());
                    wr.WriteLine(tam_3.ToString());
                    wr.WriteLine(tam_4.ToString());
                    // DINAMICO
                    wr.WriteLine(tam_1_dim.ToString());
                    wr.WriteLine(tam_2_dim.ToString());
                    wr.WriteLine(tam_3_dim.ToString());
                    wr.WriteLine(tam_4_dim.ToString());
                    wr.WriteLine(tam_en_bytes_dato.ToString());


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al crear fichero.");
                return;
            }


            // TORRE
            MemoriaTorre.MemoriaTorre torre = new MemoriaTorre.MemoriaTorre();

            // Se guarda en el mapa NUCLEO.
            torre.CompartirTamanyos(tam_1,tam_2,tam_3,tam_4,
                tam_1_dim,tam_2_dim,tam_3_dim,tam_4_dim,tam_en_bytes_dato);
            
            torre.DefinirInicioEstatica(true, "DEFECTO",
                uso_1, uso_2);
            torre.CompartirInicioEstaticaInicio("mapa_1", false);

            torre.DefinirTempranoEstatica(true, "defecto", uso_3, uso_4);
            torre.CompartirInicioEstaticaTemprana("mapa_1", uso_3, uso_4, false);

            byte[] dato_a_add = new byte[tam_en_bytes_dato];
            for (int i = 0; i < dato_a_add.Length; i++)
            {
                dato_a_add[i] = 7; // Asigna el valor a cada elemento
            }
            
            torre.DefinirInicioDinamica(tam_en_bytes_dato);
            torre.CompartirInicioDinamica("mapa_2", false);

            // Escribe en el sector y posición.
            torre.EscribirenInicioDinamica(1, 3, dato_a_add);

            byte[] dato_a_leer = new byte[tam_en_bytes_dato];
            torre.LeerenInicioDinamica(1, 3, out dato_a_leer);
            Console.WriteLine(dato_a_leer[0].ToString());

            torre.CompartirInicioDinamica("mapa_2", false);


            // Prueba. Se deberia hacer en el LectorMemorias o cualquier otro lado

            // Ejecucion
            peticion_ejecucion pet1 = new peticion_ejecucion();
            pet1.idx_mapa = 1;
            pet1.nombre_mapa = "var1";
            pet1.tam_dato = 100;
            pet1.max_nro_datos = 100;
            pet1.b_valor_defecto = true;
            pet1.caracter_defecto = '*';
            torre.AddPeticionEjecucion("var1",pet1);

            peticion_ejecucion pet2 = new peticion_ejecucion();
            pet2.idx_mapa = 2;
            pet2.nombre_mapa = "var2";
            pet2.tam_dato = 100;
            pet2.max_nro_datos = 100;
            pet2.b_valor_defecto = true;
            pet2.caracter_defecto = '*';
            torre.AddPeticionEjecucion("var2",pet2);


            //torre.CompartirEjecucion();

            byte[] datoescribir = new byte[100];
            Array.Fill(datoescribir, (byte)1);
            torre.CompartirEjecucion();

            torre.EscribirEnEjecucion("var1", 3, datoescribir);

            byte[] dato_salida;
            torre.LeerEnEjecucion("var1", 3,out dato_salida);

            Console.WriteLine("OK");

            Console.ReadKey();
            Console.ReadKey();

            torre.DestruirMapas();

        }
    }
}
