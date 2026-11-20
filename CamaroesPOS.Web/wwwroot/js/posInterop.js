window.posInterop = {
    getItem: function (key) {
        try {
            return window.localStorage.getItem(key);
        } catch {
            return null;
        }
    },
    setItem: function (key, value) {
        try {
            window.localStorage.setItem(key, value);
            return true;
        } catch {
            return false;
        }
    },
    removeItem: function (key) {
        try {
            window.localStorage.removeItem(key);
            return true;
        } catch {
            return false;
        }
    },
    printWindow: function () {
        window.print();
    }
};
