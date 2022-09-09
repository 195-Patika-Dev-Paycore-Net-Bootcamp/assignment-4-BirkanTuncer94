using HW4_BirkanTuncer.Data;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HW4_BirkanTuncer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OptimizationController : ControllerBase
    {

        private readonly ISession session;
        public List<Container> containerList;
        public List<List<Container>> clusterList;
        public OptimizationController(ISession session)
        {
            this.session = session;
            this.containerList = new List<Container>();
            this.clusterList = new List<List<Container>>();

        }


        
        
        private List<List<Container>> ClusterList(int numberOfClusters, List<Container> containerList) 
        {


            /// <summary>
            /// Genel olarak K-means formülü uyarlaması: İstenilen küme sayısı kadar rastgele nokta belirleyip ( c1, c2 .. lati = x, long = y ) bu noktalar ile, halihazırda bulunan container lokasyonlarındaki lati ve long lar arasındaki matematiksel işlemi yaparak ( mutlak değer içerisinde karelerinin farkının karekökü ) her bir c noktası için bir sonuç belirlemek. Belirlenen bu sonuç, işleme tabi tutulan c noktası ile işleme tabi tutulan container arasındaki mesafeyi vermektedir. bu mesafeleri baz alarak containerları en yakındaki kümeye atıyoruz.
            /// </summary>


            Container[,] containerArray = new Container[numberOfClusters,containerList.Count]; // Kümelere yerleşmiş olan container ları 2 boyutlu arrayde tutuyorum. Küme sayısı kadar yatayda, Listedeki container sayısı kadar dikeyde yer açıyorum. Bu sayede her bir kümeye sadece 1 eleman yerleşse de, sadece bir kümeye tüm containerlar yerleşse de; arrayin boyutu ile alakalı bir problem yaşamıyorum. Arrayin kullanımı için ufak bir örnek vermek gerekirse 2 kümemiz ve 5 containerımız olsun. c1 e iki eleman, c2 ye üç eleman yerleşecek olsun. c1 kümesi array[0,0] array[0,1] c2 kümesi array[1,0] array[1,1] array[1,2] olarak yerleşecektir. 
            
            
            int[] clusterPoints = new int[numberOfClusters*2]; // Her bir küme için lati ve long değerlerini belirliyorum. İstenilen küme sayısının 2 katı sayıda indexe sahip bir array açıyorum ve oluşturacağım C1 in latitude değeri index i, longitude değeri lenght-i değeri olarak belirliyorum. ( 3 küme varsa c1 in latisi array[0], longtitude değeri array[5], c2 için bu değerler array[1], array[4] c3 için array[2], array[3] şeklinde. )
            List<Container> orderedContainers = new List<Container>();
            List<List<Container>> clusteredList = new List<List<Container>>();

            double min = 9999999; // Containerın tüm kümelere olan uzaklıklarından minimum olanı bulmak için min başlangıç değerini inanılmaz büyük bir sayı yapıyorum. Daha sonra kümelerle olan mesafe hesaplandıkça daha yakın bir küme bulunduğunda o kümenin değeri buraya işlenecek
            double euklid = 0; // Euklid değeri container ile kümenin noktası arasındaki mesafe.
            int whichCluster = 0; // hesaplanan min değerine göre hangi kümeye yerleşeceğini tuttuğum değişken.
            int count = 0; // count değeri en son, listeye ekleme sırasında kullanılıyor
            int element = 0; // element değeri en son, listeye ekleme sırasında kullanılıyor
            bool added = false; // containerın kümeye yerleştirilip yerleştirilmediğini tuttuğum değişken
            Random rnd = new Random();

            int clusterPointMax = 0; // c1 c2 c3 ün x ve y sinin max değerini tutuyorum

            foreach (var container in containerList) // C1 C2 C3 ÜN X VE Y SİNİN ALABİLECEĞİ MAX DEĞERİ, CONTAINERLARDA BULUNAN MAX LAT VE MAX LONG DEĞERİNE GÖRE BELİRLEDİĞİM FOREACH
            {
                if(container.Latitude > clusterPointMax)
                {
                    clusterPointMax = (int)container.Latitude;
                }
                if (container.Longitude > clusterPointMax)
                {
                    clusterPointMax = (int)container.Longitude;
                }
            }

            #region Cluster noktalarının ( c1,c2... ) belirlenmesi.

            for (int i = 0; i < clusterPoints.Length; i++)
            {
                clusterPoints[i] = rnd.Next(0, clusterPointMax); /// c1, c2 gibi noktaların lati ve long değerlerini tuttuğum array e rastgele sayı ataması yaptığım yer. Bu sayede c1 c2 c3 vb noktaları rastgele belirliyorum.
            }

            #endregion


            foreach (var item in containerList) // İşleme tabi tutulacak tüm containerları foreach ile dönüyorum.
            {

                #region Containerlar ile cluster noktalarının arasındaki mesafenin hesaplanması

                for (int i = 0; i < numberOfClusters; i++) // İŞLEME TABİ TUTULAN CONTAİNERLARIN C1 C2 C3 VB NOKTALARI İLE ARASINDAKİ MESAFEYİ HESAPLADIĞIM FOR DÖNGÜSÜ  
                {
                    euklid = Math.Abs(Math.Pow(item.Latitude, 2) - Math.Pow(clusterPoints[i], 2)); // arasındaki mesafeyi hesapladığım formül.
                    if (euklid < min) // diğer kümeler ile olan mesafesinden kısa ise hangi kümeye yerleştirileceğini atadığım yer.
                    {
                        min = euklid;
                        whichCluster = i;
                    }

                }
                #endregion


                #region Kümeye yerleştirme işlemleri

                for (int i = 0; i < numberOfClusters; i++) // HANGİ KÜMEYE YERLEŞTİRİLECEĞİ BELİRLENMİŞ OLAN CONTAINERIN, İKİ BOYUTLU ARRAYDE YERİNİN HESAPLANACAĞI FOR DÖNGÜSÜ
                {
                    for (int j = 0; j < containerList.Count; j++)
                    {
                        if (containerArray[i, j] == null && whichCluster == i && added == false) // iki boyutlu arrayin yatay kısmı(i) hangi kümeye ait olduğu, dikey kısmı(j) ise containerın yerleştirileceği yeri belirliyor. bu if statement kısmında kontrol edilen dikey kısmın dolu olup olmadığını kontrol ediyorum. doluysa bir sonraki dikey indexe bakacak. boş yer bulana kadar yani. Eğer boş yer bulup oraya yerleştirirse de "added" oalrak belirlediğim değişken true olacak ve bu if statement a bir daha girmeyecek. 
                        {
                            containerArray[i, j] = item;
                            added = true;

                        }
                    }
                }

                #endregion


                min = 999999;
                added = false; //// hesaplamalar yapılıp container gereken yere yerleştirildikten sonra min ve added değişkenleri eski haline geri dönüyor ve bir sonraki container için aynı işlemler yapılmaya devam ediyor.
            }

            #region Kümelerin listeye eklenmesi

            for (int i = 0; i < numberOfClusters; i++) // TÜM CONTAINERLAR İKİ BOYUTLU OLAN ARRAY E, KÜMELENMİŞ BİR ŞEKİLDE YERLEŞTİRİLDİKTEN SONRA HER BİR KÜMENİN ELEMANLARI TEKER TEEKR LSİTEYE AKTARILDIĞI FOR DÖNGÜSÜ
            {
                for (int j = 0; j < containerList.Count; j++)
                {
                    if (containerArray[i, j] != null) // dikey pozisyonun null olup olmadığını kontrol ediyorum. null olduğu anda o kümede başka eleman kalmamış demektir.
                    {
                        orderedContainers.Add(containerArray[i, j]); // null değil ise listeye atıyorum.
                        count++; // aynı kümenin kaç adet elemanı var bu bilgiyi tuttuğum değişken.
                        element++; // toplam kaç tane container işleme tabi tutuluyor bu bilgiyi tuttuğum değişen. // NOT = bölme işlemini getrange kullanarak yapıyorum. count ve element sayılarını tutma sebebim bu. 
                    }
                }
                clusteredList.Add(orderedContainers.GetRange(element - count, count)); // Genel mantık şu şekilde = tüm containerları tek bir listeye küme1in elemanları, küme ikininelemanları vb şeklinde SIRAYLA atıyorum. daha sonradan count ve element değişkenlerini kullanarak düzgün bir şekilde parçalara ayırıp container listesi tipinde bir listeye ( son kullanıcının gördüğü ) yerleştiriyorum.
                count = 0;
            }

            #endregion

            return clusteredList;
        }





        [HttpGet("ClusterKMeans")]
        public List<List<Container>> Get(int id, int numberOfClusters)
        {

            var containersWillDistribute = session.Query<Container>().Where(x => x.VehicleId == id).ToList(); 
              

            return ClusterList(numberOfClusters, containersWillDistribute);
        } 

    }
}
