window.indexedDbStorage = {
    // Open the database (creates it if it doesn't exist, or upgrades if needed)
    open: function (dbName = "AppDb") {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(dbName, 1);

            // Create object store if needed during database upgrade
            request.onupgradeneeded = function (event) {
                const db = event.target.result;
                if (!db.objectStoreNames.contains('cache')) {
                    db.createObjectStore('cache', { keyPath: 'key' });
                }
            };

            request.onsuccess = function (event) {
                resolve(event.target.result); // Resolve with the database
            };

            request.onerror = function (event) {
                reject(event.target.error); // Reject with the error
            };
        });
    },

    // Save data to the cache store (with JSON serialization)
    saveData: function (dbName, key, value) {
        return this.open(dbName).then(db => {
            return new Promise((resolve, reject) => {
                const transaction = db.transaction('cache', 'readwrite');
                const store = transaction.objectStore('cache');
                const serializedValue = JSON.stringify(value); // Serialize the value

                const request = store.put({ key: key, value: serializedValue }); // put() will overwrite if key exists

                request.onsuccess = () => resolve();
                request.onerror = () => reject(request.error);
            });
        });
    },

    // Get data from the cache store (with JSON deserialization)
    getData: function (dbName, key) {
        return this.open(dbName).then(db => {
            return new Promise((resolve, reject) => {
                const transaction = db.transaction('cache', 'readonly');
                const store = transaction.objectStore('cache');
                const request = store.get(key);

                request.onsuccess = function (event) {
                    const result = event.target.result;
                    if (result) {
                        try {
                            resolve(JSON.parse(result.value)); // Deserialize the value
                        } catch (e) {
                            reject("Error parsing JSON");
                        }
                    } else {
                        resolve(null); // No data found
                    }
                };

                request.onerror = function (event) {
                    reject(event.target.error);
                };
            });
        });
    },

    // Clear all data from the cache store
    clearData: function (dbName) {
        return this.open(dbName).then(db => {
            return new Promise((resolve, reject) => {
                const transaction = db.transaction('cache', 'readwrite');
                const store = transaction.objectStore('cache');
                const request = store.clear(); // Clear all data in the store

                request.onsuccess = function () {
                    resolve(); // Success, data cleared
                };

                request.onerror = function (event) {
                    reject(event.target.error); // Error clearing data
                };
            });
        });
    }
};
