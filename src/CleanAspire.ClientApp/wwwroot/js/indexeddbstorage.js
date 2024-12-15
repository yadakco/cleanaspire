window.indexedDbStorage = {
    // Open the database (creates it if it doesn't exist, or upgrades if needed)
    open: function (dbName = "AppDb") {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(dbName, 2); // Increment version to 2

            // Create object store if needed during database upgrade
            request.onupgradeneeded = function (event) {
                const db = event.target.result;
                if (!db.objectStoreNames.contains('cache')) {
                    db.createObjectStore('cache', { keyPath: 'key' });
                } else {
                    const store = event.currentTarget.transaction.objectStore('cache');
                    if (!store.indexNames.contains('tags')) {
                        store.createIndex('tags', 'tags', { multiEntry: true });
                    }
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

    // Save data to the cache store (with JSON serialization and optional tags)
    saveData: function (dbName, key, value, tags = []) {
        return this.open(dbName).then(db => {
            return new Promise((resolve, reject) => {
                const transaction = db.transaction('cache', 'readwrite');
                const store = transaction.objectStore('cache');
                const serializedValue = JSON.stringify(value); // Serialize the value

                const request = store.put({ key: key, value: serializedValue, tags: tags }); // Include tags

                request.onsuccess = () => resolve();
                request.onerror = () => reject(request.error);
            });
        });
    },

    // Get all data by tags (array of tags)
    getDataByTags: function (dbName, tags) {
        return this.open(dbName).then(db => {
            return new Promise((resolve, reject) => {
                const transaction = db.transaction('cache', 'readonly');
                const store = transaction.objectStore('cache');
                const index = store.index('tags');

                const results = [];
                const tagRequests = tags.map(tag => {
                    return new Promise((tagResolve, tagReject) => {
                        const request = index.getAll(tag);

                        request.onsuccess = function (event) {
                            results.push(...event.target.result);
                            tagResolve();
                        };

                        request.onerror = function (event) {
                            tagReject(event.target.error);
                        };
                    });
                });

                Promise.all(tagRequests)
                    .then(() => resolve(results.map(entry => ({ key: entry.key, value: JSON.parse(entry.value) }))))
                    .catch(reject);
            });
        });
    },

    // Delete all data with specific tags (array of tags)
    deleteDataByTags: function (dbName, tags) {
        return this.open(dbName).then(db => {
            return new Promise((resolve, reject) => {
                const transaction = db.transaction('cache', 'readwrite');
                const store = transaction.objectStore('cache');
                const index = store.index('tags');

                const deletePromises = [];
                const tagRequests = tags.map(tag => {
                    return new Promise((tagResolve, tagReject) => {
                        const request = index.getAllKeys(tag);

                        request.onsuccess = function (event) {
                            const keys = event.target.result;
                            const keyDeletes = keys.map(key => {
                                return new Promise((delResolve, delReject) => {
                                    const deleteRequest = store.delete(key);
                                    deleteRequest.onsuccess = () => delResolve();
                                    deleteRequest.onerror = () => delReject(deleteRequest.error);
                                });
                            });
                            deletePromises.push(...keyDeletes);
                            tagResolve();
                        };

                        request.onerror = function (event) {
                            tagReject(event.target.error);
                        };
                    });
                });

                Promise.all(tagRequests)
                    .then(() => Promise.all(deletePromises))
                    .then(resolve)
                    .catch(reject);
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
    },

    // Delete a specific key from the cache store
    deleteData: function (dbName, key) {
        return this.open(dbName).then(db => {
            return new Promise((resolve, reject) => {
                const transaction = db.transaction('cache', 'readwrite');
                const store = transaction.objectStore('cache');
                const request = store.delete(key); // Delete the specified key

                request.onsuccess = function () {
                    resolve(); // Success, key deleted
                };

                request.onerror = function (event) {
                    reject(event.target.error); // Error deleting key
                };
            });
        });
    }
};
