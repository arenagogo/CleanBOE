//
// FirebaseBridge.jslib
//
mergeInto(LibraryManager.library, {
    /**
     * Mengambil satu dokumen spesifik dari Firestore berdasarkan ID-nya.
     * @param {string} collectionPath - Nama koleksi (misal: "users").
     * @param {string} docId - ID Dokumen (ini adalah userId Anda).
     * @param {string} gameObjectName - Nama GameObject di Unity untuk menerima callback.
     * @param {string} successCallback - Nama fungsi di C# jika berhasil.
     * @param {string} errorCallback - Nama fungsi di C# jika gagal.
     */
    GetDataFromFirestore: function (collectionPath, docId, gameObjectName, successCallback, errorCallback) {
        const path = UTF8ToString(collectionPath);
        const id = UTF8ToString(docId);
        const objectName = UTF8ToString(gameObjectName);
        const successFunc = UTF8ToString(successCallback);
        const errorFunc = UTF8ToString(errorCallback);

        firebase.firestore().collection(path).doc(id).get()
            .then((doc) => {
                if (doc.exists) {
                    // Jika dokumen ditemukan, kirim datanya sebagai string JSON.
                    const jsonData = JSON.stringify(doc.data());
                    if (unityInstance) {
                        unityInstance.SendMessage(objectName, successFunc, jsonData);
                    }
                } else {
                    // Jika dokumen tidak ditemukan, kirim pesan error.
                    if (unityInstance) {
                        unityInstance.SendMessage(objectName, errorFunc, "Error: Dokumen dengan ID tersebut tidak ditemukan.");
                    }
                }
            })
            .catch((error) => {
                // Jika terjadi error lain (izin, koneksi), kirim pesan error.
                if (unityInstance) {
                    unityInstance.SendMessage(objectName, errorFunc, error.message);
                }
            });
    }
});