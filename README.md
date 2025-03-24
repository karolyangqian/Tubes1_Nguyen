# Tugas Besar 1 - IF2211 Strategi Algoritma
> Penerapan Strategi Algoritma Greedy pada permainan Robocode Tank Royale.

Tugas besar ini bertujuan untuk mengeksplorasikan dan mengimplementasikan salah satu konsep strategi algoritma yaitu strategi _greedy_ pada robot yang dikembangkan dalam permainan Robocode Tank Royale. 

## Kelompok 55 - Nguyen
foto
| NIM | Nama |
| :---: | :---: |
| 13523089 | Ahmad Ibrahim |
| 13523093 | Karol Yangqian Poetracahya |
| 13523112 | Aria Judhistira |

## Instalasi / Memulai
Untuk memulai proyek ini, silakan lakukan _cloning_ repository ini dengan menjalankan perintah berikut pada terminal.
```sh
git clone https://github.com/karolyangqian/Tubes1_Nguyen.git
cd Tubes1_Nguyen
```

### Dependensi
Sebelum menjalankan program, pastikan bahwa Anda sudah menginstal Java dan Dotnet sebelumnya.

### Struktur Program
```bash
├── src/
│   ├── Qwuck/…
│   └── alternative-bots/
│       ├── Schmelly/…
│       ├── Pffrrrhh/…
│       └── Woff/…
├── doc/
│   └── Nguyen.pdf
└── README.md
```

### Menjalankan Program
1. Untuk menjalankan program, silakan jalankan file robocode-tankroyale-gui-0.30.0.jar atau mengetik perintah di bawah ini: <br>
Pastikan Anda berada pada direktori Tubes1_Nguyen.
```sh
java -jar robocode-tankroyale-gui-0.30.0.jar
```
2. Klik tombol "Config" pada aplikasi, kemudian klik "Bot Root Directories" dan masukkan jalur menuju folder robot yang ingin dimainkan.
3. Klik tombol "Battle", kemudian klik "Start Battle" untuk memunculkan antarmuka konfigurasi pertandingan.
4. Pilih robot yang ingin dimainkan, kemudian klik "Boot" pada bagian atas. Setelah selesai melakukan _booting_, robot yang ditambahkan akan muncul pada bagian bawah. Pilih robot dan klik "Add" untuk menambahkan robot ke dalam permainan.
5. Klik "Start Battle" untuk memulai permainan.
6. Anda juga dapat mengonfigurasi peraturan permainan, seperti jumlah ronde, jumlah minimum robot, dimensi arena permainan, dll. pada bagian "Setup Rules".

## Deksripsi Robot
Pada repositori ini, terdapat 4 robot yang kami kembangkan, yakni Qwuck, Schmelly, Pffrrrhh, dan Woff.
* **Qwuck**: Robot utama kami, strategi yang digunakan Qwuck adalah mencari pojok arena dengan risiko minimum. Setelah mencapai pojok arena tersebut, Qwuck akan melakukan pergerakan melingkar dan menargetkan musuh dengan Energy yang terkecil.
* **Schmelly**: Schmelly mengandung implementasi pergerakan Anti-Gravity yang akan menjauhi robot yang telah ia pindai. Schmelly akan mengikuti arah gaya vektor musuh dengan harapan dapat menemukan posisi dengan risiko terkecil.
* **Pffrrrhh**: Pffrrrhh menganut prinsip "High Risk, High Reward". Sederhananya, ia akan mengincar robot yang ia pindai dan melakukan _ramming_ sekaligus menembaknya. Motivasi robot ini adalah memaksimumkan pengambilan poin tanpa memperhitungkan keselamatannya.
* **Woff**: isi sendiri

## Tautan
(isi link yt)
