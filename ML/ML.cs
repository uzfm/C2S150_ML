using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

using Emgu.CV;
using Emgu.CV.Structure;



using Google.Protobuf;
using Tensorflow;
using Tensorflow.Keras;
using Tensorflow.Keras.Engine;
using static Tensorflow.KerasApi;
using static Tensorflow.Binding;
using Tensorflow.NumPy;
using Emgu.CV.CvEnum;

namespace C2S150_ML
{



   public class  ML
    {



            int batch_size = 32;
            int ColourChanal = 1;
            int epochs = 50;
            Shape img_dim = (64, 64);
            IDatasetV2 train_ds, val_ds;
          public  Model model;   
           int num_classes = 2;

       string ModelaPach="";

     public Model ReadModel() {

            return model;
        }



      public void BuildModel()   {
      var layers = keras.layers;

            //  var normalization_layer = tf.keras.layers.Rescaling(1.0f / 255);

            // Нормалізація даних
            // var normalization_layer = KerasApi.keras.layers.Rescaling(1.0f / 255);


            //model = keras.Sequential(new List<ILayer>{

            //    layers.Rescaling(1.0f / 255, input_shape: (img_dim.dims[0], img_dim.dims[1], ColourChanal)),
            //    layers.Conv2D(32, 3, padding: "same", activation: keras.activations.Relu),
            //    layers.MaxPooling2D(),
            //    layers.Conv2D(64, 3, padding: "same", activation: keras.activations.Relu),
            //    layers.MaxPooling2D(),
            //    layers.Conv2D(128, 3, padding: "same", activation: keras.activations.Relu),
            //    layers.MaxPooling2D(),
            //    layers.Flatten(),
            //    layers.Dropout(0.5f),
            //    layers.Dense(256, activation: keras.activations.Relu),
            //    layers.Dense(num_classes)

            //});


            // Створюємо модель за допомогою TensorFlow.Keras.Sequential
            model = keras.Sequential(new List<ILayer> {
    // Нормалізація пікселів
    layers.Rescaling(1.0f / 255, input_shape: (img_dim.dims[0], img_dim.dims[1], ColourChanal)),
    // Перший шар згорткової мережі з 32 фільтрами та розміром ядра 3х3
    layers.Conv2D(32, 3, padding: "same", activation: keras.activations.Relu),
    // Шар пулінгу, що зменшує розмірність зображення в 2 рази
    layers.MaxPooling2D(),
    // Другий шар згорткової мережі з 64 фільтрами та розміром ядра 3х3
    layers.Conv2D(64, 2, padding: "same", activation: keras.activations.Relu),
    // Шар пулінгу
    layers.MaxPooling2D(),
    // Розгладжуємо отриманий тензор
    layers.Flatten(),
    // Випадково відключаємо 50% нейронів
    layers.Dropout(0.5f),
    // Повнозв'язний шар з 256 нейронами та функцією активації ReLU
    layers.Dense(256, activation: keras.activations.Relu),
    // Вихідний шар з кількістю нейронів, рівною кількості класів
    layers.Dense(num_classes)

});

            var  model1 = keras.Sequential(new List<ILayer>{
            // Нормалізація вхідних даних
            layers.Rescaling(1.0f / 255, input_shape: (img_dim.dims[0], img_dim.dims[1], ColourChanal)),
            
            // Перший блок Conv2D та MaxPooling2D шарів
            layers.Conv2D(32, 3, padding: "same", activation: keras.activations.Relu),
            layers.BatchNormalization(),
            layers.Conv2D(32, 3, padding: "same", activation: keras.activations.Relu),
            layers.BatchNormalization(),
            layers.MaxPooling2D(),
            layers.Dropout(0.25f),
            
            // Другий блок Conv2D та MaxPooling2D шарів
            layers.Conv2D(64, 3, padding: "same", activation: keras.activations.Relu),
            layers.BatchNormalization(),
            layers.Conv2D(64, 3, padding: "same", activation: keras.activations.Relu),
            layers.BatchNormalization(),
            layers.MaxPooling2D(),
            layers.Dropout(0.25f),

            // Третій блок Conv2D та MaxPooling2D шарів
            layers.Conv2D(128, 3, padding: "same", activation: keras.activations.Relu),
            layers.BatchNormalization(),
            layers.Conv2D(128, 3, padding: "same", activation: keras.activations.Relu),
            layers.BatchNormalization(),
            layers.MaxPooling2D(),
            layers.Dropout(0.25f),

            // Перетворення вхідних даних в одновимірний вектор
            layers.Flatten(),
            
            // Повнозв'язаний шар
            layers.Dense(512, activation: keras.activations.Relu),
            layers.BatchNormalization(),
            layers.Dropout(0.5f),
            
            // Вихідний шар з 2-ма нейронами (боби та дефекти)
            layers.Dense(2, activation: keras.activations.Softmax)
        });







            model.compile(optimizer: keras.optimizers.Adam(),
                loss: keras.losses.SparseCategoricalCrossentropy(from_logits: true),
                metrics: new[] { "accuracy" } 

                );

          

            model.summary();
        }

        Model ResFast32()
        {


            var layers = keras.layers;
            // input layer
            var inputs = keras.Input(shape: (img_dim.dims[0], img_dim.dims[1], ColourChanal), name: "img");

            //var inputs = layers.Rescaling(1.0f / 255).Apply(input);


            // convolutional layer
            var x = layers.Conv2D(32, 3, activation: "relu").Apply(inputs);
            x = layers.Conv2D(32, 3, activation: "relu").Apply(x);
            var block_1_output = layers.MaxPooling2D(3).Apply(x);

            x = layers.Conv2D(32, 3, activation: "relu", padding: "same").Apply(block_1_output);
            x = layers.Conv2D(32, 3, activation: "relu", padding: "same").Apply(x);
            var block_3_output = layers.Add().Apply(new Tensors(x, block_1_output));

            x = layers.Conv2D(32, 3, activation: "relu").Apply(block_3_output);
            x = layers.GlobalAveragePooling2D().Apply(x);
            x = layers.Dense(128, activation: "relu").Apply(x);
            x = layers.Dropout(0.5f).Apply(x);
            // output layer
            var outputs = layers.Dense(num_classes, activation: keras.activations.Softmax).Apply(x);
            // build keras model
            model = keras.Model(inputs, outputs, name: "black_spot_detector");
            model.summary();

            // compile keras model in tensorflow static graph
            model.compile(optimizer: keras.optimizers.RMSprop(1e-6f),
                loss: keras.losses.SparseCategoricalCrossentropy(from_logits: true),
                  metrics: new[] { "accuracy" /*  "acc"*/ });




            return model;
        }






        public void ReloadModel()
        {
            model.compile(optimizer: keras.optimizers.Adam(),
            loss: keras.losses.SparseCategoricalCrossentropy(from_logits: true),
            metrics: new[] { "accuracy" }
            );

            model.load_weights(ModelaPach);
        }


        //    public void Inst( string DataPash){

        //    PrepareData(DataPash);
        //    BuildModel();
        //    Train(DataPash);

        //}

        public void InstModel(string DataPash)
        {

            ResFast32();
            //BuildModel();
            ModelaPach = @DataPash + "\\"+STGS.Data.ML_NAME+".h5";
            model.load_weights(ModelaPach);

          
            // Створити сіру картинку 64x64
            Image<Gray, byte> grayImage = new Image<Gray, byte>((int) img_dim[0],(int) img_dim[1]);
            // Заповнити картинку сірим кольором
            grayImage.SetValue(new Gray(128));


            Mat mat = grayImage.Mat;
       

            List<Mat> Img = new List<Mat>();
            Img.Add(mat);
            Img.Add(mat);
            Img.Add(mat);

            var test=  PredictImage(Img);

        }


        public void Train(string DataPash)
        {
          
        // Model.;

            ///////////////////////////////////////var classFile = Path.Combine(@"class_names.txt");
            // model.fit(train_ds, validation_data: val_ds, epochs: epochs );
            //model.save_weights(@DataPash+ "save_format.h5", save_format:"h5");  
            //keras.models.load_model (@"D:\C2_MLNET\TenserflowKeras  test CUDA11.2_NDN8.1_Grey\bin\Debug\net5.0-windows\Model.h6");
            //model. load_weights(@"D:\C2_MLNET\TenserflowKeras  test CUDA11.2_NDN8.1_Grey\bin\Debug\net5.0-windows\Model.h6\saved_model");

             ModelaPach = @DataPash + "\\"+STGS.Data.ML_NAME+".h5";
            model.load_weights(ModelaPach);
            //keras.models.load_model (@"D:\C2_MLNET\TenserflowKeras  test CUDA11.2_NDN8.1_Grey\bin\Debug\net5.0-windows\Model.h6\saved_model");
            //Console.WriteLine("Load Model: -- " + watch.ElapsedMilliseconds + " ms");

        }


        public void PredictImage(List<Mat> Imgs , out Tensor value)
        {
            var images = new List<Tensor>();

            Shape shape = (1, img_dim[0], img_dim[1], ColourChanal);

            foreach (var mat in Imgs)
            {

                var matCopy = mat.ToImage<Rgb, byte>().Resize((int)img_dim.dims[0], (int)img_dim.dims[0], Emgu.CV.CvEnum.Inter.Linear);

                if (mat.Dims != 3) { CvInvoke.CvtColor(matCopy, mat, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray); }


                byte[] dataset = mat.GetRawData();
                float[] floatImageDataFloat = dataset.Select(b => (float)b).ToArray();

                images.Add(tf.constant(floatImageDataFloat, TF_DataType.TF_FLOAT, shape, name: "Const"));
            }

            var imageTe = tf.stack(images);
            var path_ds = tf.data.Dataset.from_tensor_slices(imageTe);


             value = model.predict(path_ds, batch_size: 32);


            //  var numpyArray = value[0].numpy();
            // var class_index = np.argmax(numpyArray[0]);

            //Console.WriteLine(/*class_index.ToString() */ "Prediction:" + numpyArray.ToString() + "---");
            //Console.WriteLine(/*class_index.ToString() */ "Prediction:" + class_index.ToString() + "------" + elapsedMs.ToString() + " ms");

            
        }

      public    Tensor PredictImage(List<Mat> Imgs )
        {

                var images = new List<Tensor>();

                Shape shape = (1, img_dim[0], img_dim[1], ColourChanal);
                   
                bool Resize = false;
                if (Imgs[0].Width != img_dim[0]) { Resize = true; }
           
                foreach (var mat in Imgs){
                if (Resize)
                {
                    var matCopy = mat.ToImage<Rgb, byte>().Resize((int)img_dim.dims[0], (int)img_dim.dims[0], Emgu.CV.CvEnum.Inter.Linear);
                    CvInvoke.CvtColor(matCopy, mat, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                }
                else { CvInvoke.CvtColor(mat.ToImage<Rgb, byte>(), mat, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray); }
                 

                    //Mat dst = new Mat();

                    //CvInvoke.Resize(mat, dst, new Size((int)img_dim[0], (int)img_dim[1]), 0, 0, Inter.Linear);

                    byte[] dataset = mat.GetRawData();

                    float[] floatImageDataFloat = dataset.Select(b => (float)b).ToArray();

                    images.Add(tf.constant(floatImageDataFloat, TF_DataType.TF_FLOAT, shape, name: "Const"));
                }

                var imageTe = tf.stack(images);
                var path_ds = tf.data.Dataset.from_tensor_slices(imageTe);


                var value = model.predict(path_ds, use_multiprocessing: true);


                //  var numpyArray = value[0].numpy();
                // var class_index = np.argmax(numpyArray[0]);

                //Console.WriteLine(/*class_index.ToString() */ "Prediction:" + numpyArray.ToString() + "---");
                //Console.WriteLine(/*class_index.ToString() */ "Prediction:" + class_index.ToString() + "------" + elapsedMs.ToString() + " ms");

            return value;
        }


        public void PredictImage(Mat mat)
        {


 

            CvInvoke.Resize(mat, mat, new Size((int)img_dim.dims[0], (int)img_dim.dims[0]));

            // Convert the image to RGB format if it is not already in that format.
            if (mat.Dims != 3)
            {
                CvInvoke.CvtColor(mat, mat, Emgu.CV.CvEnum.ColorConversion.Bgr2Rgb);
            }

            float[] floatImageDataFloat = mat.GetRawData().Select(b => (float)b).ToArray();
            Shape ShapeS = new Shape(1, (int)img_dim.dims[0], (int)img_dim.dims[0], 3);
            var TensorByte = tf.constant(floatImageDataFloat, TF_DataType.TF_FLOAT, ShapeS);
            var input = tf.expand_dims(TensorByte, 0);
            var path_ds = tf.data.Dataset.from_tensor_slices(input);
            //var path_ds = tf.data.Dataset.from_tensor_slices(tf.constant(floatImageDataFloat, TF_DataType.TF_FLOAT, ShapeS));
            //path_ds = path_ds.shuffle(32 * 8, seed: 123);
            // path_ds = path_ds.batch(32);

            Tensor predictions = model.predict((Tensor)path_ds, batch_size: 32, use_multiprocessing: true);

            //-0.6509194] false
            //-0.6893591 true

            // Перетворити результати передбачення в NumPy array
            var numpyArray = predictions.numpy();
            // Обчислити softmax на виході з моделі
            //var class_index =  tf.nn.softmax(numpyArray[0]);

            var class_index = np.argmax(numpyArray[0]);


            // return Pdt.SoftMax(predictions);
        }

      //"D:\\V2\\C2S150\\C2S150_ML\\bin\\Debug\\net5.0-windows\\Data"

        public void PrepareData(string data_dir )
        {
            //var   rthrf=train_ds.ToArray()[0].Item1; // одна картинка



            string[] uihu = new string[2];
            uihu[0] = @"data\Blight\2.18.2021. 1.56.12 img0.jpg";
            uihu[1] = @"data\Blight\2.18.2021. 1.56.12 img1.jpg";


            int[] intuihu = new int[2];
            intuihu[0] = 1;
            intuihu[1] = 2;
            // var val_ds2 = keras.preprocessing.paths_and_labels_to_dataset(uihu, intuihu, "lab1", 2, 1);



            string fileName = "flower_photos.tgz";
            //string url = $"https://storage.googleapis.com/download.tensorflow.org/example_images/flower_photos.tgz";


            //string data_dir = Path.Combine(Path.GetTempPath(), "flower_photos");
            //Web.Download(url, data_dir, fileName);
            //Compress.ExtractTGZ(Path.Join(data_dir, fileName), data_dir);
            //data_dir = Path.Combine(data_dir, "flower_photos");
            string[] ddd = new string[2];
            ddd[0] = data_dir;
            ddd[1] = data_dir;



            // convert to tensor
            train_ds = KerasApi.keras.preprocessing.image_dataset_from_directory(data_dir,
                validation_split: 0.2f,
                subset: "training",
                color_mode: "grayscale",
                //color_mode: "rgb",
                seed: 123,
                image_size: img_dim,
                batch_size: batch_size
                );




            val_ds = KerasApi.keras.preprocessing.image_dataset_from_directory(data_dir,
            validation_split: 0.2f,
            subset: "validation",
            color_mode: "grayscale",
            //color_mode: "rgb",
            seed: 123,
            image_size: img_dim,
            batch_size: batch_size);



            //foreach (var (imageBatch, labelBatch) in train_ds)
            //{
            //    var labelBatchArray = labelBatch.numpy();

            //    for (var i = 0; i < labelBatchArray.ToByteArray().Length; i++)
            //    {
            //        var classIndex = labelBatchArray[i];

            //        //var className = classNames[classIndex];

            //        // Далі можна використовувати назву папки та номер класу
            //        // для потреб проекту.
            //    }
            //}

            // var class_names = train_ds.   class_names;




            //foreach (var (image, label) in train_ds.AsEnumerable())
            //{
            //    //image.i
            //    //var path = train_ds.FilePaths.ElementAtOrDefault(i);
            //    //var class_name = class_names[label.value_index];
            //    //Console.WriteLine($"Path: {path} | Class: {class_name} ({label})");
            //}




             train_ds = train_ds.shuffle(1000).prefetch(buffer_size: -1);
            val_ds = val_ds.prefetch(buffer_size: -1);
        }




    }



}
