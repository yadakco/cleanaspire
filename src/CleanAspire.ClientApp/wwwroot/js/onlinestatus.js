export function getOnlineStatus() {
    return navigator.onLine;
}
export function addOnlineStatusListener(dotNetObjectRef) {
    window.addEventListener('online', () => {
        dotNetObjectRef.invokeMethodAsync('UpdateOnlineStatus', true);
    });
    window.addEventListener('offline', () => {
        dotNetObjectRef.invokeMethodAsync('UpdateOnlineStatus', false);
    });
}