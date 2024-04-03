using MediaToolkit;
using MediaToolkit.Model;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;


namespace ModulacionAudioVideo
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void BtnModular_Click(object sender, EventArgs e)
        {
            try
            {
                if(FlArchivoModular.HasFile && (FlArchivoModular.FileName.ToLower().EndsWith(".mp3") || FlArchivoModular.FileName.ToLower().EndsWith(".mp4")))
                {

                    if(FlArchivoModular.FileName.ToLower().EndsWith(".mp3"))
                    {
                        // logica de audios
                        using (MemoryStream stream = new MemoryStream(FlArchivoModular.FileBytes))
                        {
                            using (var mp3File = new Mp3FileReader(stream))
                            {
                                TimeSpan duration = mp3File.TotalTime;

                                if (duration.TotalSeconds > 32)
                                {
                                    Response.Write("<script>alert('El archivo de audio es mayor a 30 segundo por favor suba otro audio');</script>");
                                }
                                else
                                {
                                    // convertir el audio en secuencia de bits para la modulación
                                    bool[] bitsAudio = ConvertirAudioABits(mp3File);

                                    double[] FasesAmplitudes = ModulacionQAM(bitsAudio, 26); 

                                }

                            }
                        }
                    }
                    else
                    {
                        // logica de videos
                        string tempFilePath = Path.GetTempFileName();
                        File.WriteAllBytes(tempFilePath, FlArchivoModular.FileBytes);

                        using (var engine = new Engine())
                        {
                            MediaFile inputFile = new MediaFile { Filename = tempFilePath };
                            engine.GetMetadata(inputFile);
                            TimeSpan duration = inputFile.Metadata.Duration;

                            if (duration.Seconds > 63)
                            {
                                Response.Write("<script>alert('El archivo que debe no debe tener un duracion mayor a 1 minuto');</script>");
                            }
                            else
                            {
                               
                                ////descargando el archivo txt de la modulacion del archivo
                                //using (MemoryStream ms = new MemoryStream(txt))
                                //{
                                //    // Limpiar cualquier contenido actual en el response
                                //    Response.Clear();

                                //    // Establecer el tipo de contenido y los encabezados para la descarga
                                //    Response.ContentType = "text/plain";
                                //    Response.AppendHeader("Content-Disposition", "attachment; filename=" + FlArchivoModular.FileName + "Modulado.txt");
                                //    Response.Buffer = true;

                                //    // Escribir los bytes en el response
                                //    Response.OutputStream.Write(txt, 0, txt.Length);

                                //    // Finalizar la respuesta
                                //    Response.Flush();
                                //    Response.SuppressContent = true;
                                //    HttpContext.Current.ApplicationInstance.CompleteRequest();
                                //}
                            }

                        }
                    }

                 
                }
                else
                {
                    Response.Write("<script>alert('El archivo que debe subir es en formato mp3 o mp4');</script>");
                }
            }catch(Exception exc)
            {
                Response.Write("<script>alert('Se presento un erro al modular el archivo que acaba de subir');</script>");
            }
        }

        // Función para convertir un archivo de audio MP3 a una secuencia de bits
        private bool[] ConvertirAudioABits(Mp3FileReader mp3File)
        {
            var buffer = new byte[1024]; // Buffer de lectura
            var bits = new List<bool>();

            // Leer el archivo de audio MP3 en bloques y convertir cada muestra de audio a una secuencia de bits
            int bytesRead;
            while ((bytesRead = mp3File.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    bits.AddRange(ConvertirByteABits(buffer[i]));
                }
            }

            return bits.ToArray();
        }

        // Función para convertir un byte a una secuencia de bits
        private IEnumerable<bool> ConvertirByteABits(byte b)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return ((b >> (7 - i)) & 1) == 1;
            }
        }

        public double[] ModulacionQAM(bool[] bits, int numBitsPorSimbolo)
        {
            // Calcular el número total de símbolos QAM
            int totalSimbolos = bits.Length / numBitsPorSimbolo;

            // Definir la tabla de constelación de QAM dinámicamente
            double[,] constelacionQAM = GenerarTablaConstelacionQAM(numBitsPorSimbolo);

            double[] amplitudes = new double[totalSimbolos];
            double[] fases = new double[totalSimbolos];

            // Mapear cada grupo de bits a un símbolo QAM y asignar amplitud y fase
            for (int i = 0; i < totalSimbolos; i++)
            {
                // Obtener el índice del símbolo en la tabla de constelación
                int indice = ConvertirBitsASimbolo(bits.Skip(i * numBitsPorSimbolo).Take(numBitsPorSimbolo).ToArray());

                // Asignar amplitud y fase del símbolo
                amplitudes[i] = constelacionQAM[indice, 0];
                fases[i] = constelacionQAM[indice, 1];
            }

            // Devolver amplitudes y fases de los símbolos QAM
            return amplitudes.Concat(fases).ToArray();
        }

        private double[,] GenerarTablaConstelacionQAM(int numBitsPorSimbolo)
        {
            int totalSimbolos = (int)Math.Pow(2, numBitsPorSimbolo); // Número total de símbolos QAM

            // Crear una matriz para almacenar la tabla de constelación
            double[,] constelacionQAM = new double[totalSimbolos, 2];

            // Llenar la tabla de constelación con valores aleatorios (deberías adaptar esto según tu requisito)
            Random rnd = new Random();
            for (int i = 0; i < totalSimbolos; i++)
            {
                constelacionQAM[i, 0] = rnd.NextDouble() * 10 - 5; // Amplitud en el rango de -5 a 5 (puedes ajustar según sea necesario)
                constelacionQAM[i, 1] = rnd.NextDouble() * 10 - 5; // Fase en el rango de -5 a 5 (puedes ajustar según sea necesario)
            }

            return constelacionQAM;
        }

        // Función para convertir un grupo de bits a un índice de símbolo en la tabla de constelación
        private int ConvertirBitsASimbolo(bool[] bits)
        {
            int indice = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    indice += (int)Math.Pow(2, bits.Length - 1 - i);
            }
            return indice;
        }
    }
}