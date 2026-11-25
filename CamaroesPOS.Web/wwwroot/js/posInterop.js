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
    getSessionItem: function (key) {
        try {
            return window.sessionStorage.getItem(key);
        } catch {
            return null;
        }
    },
    setSessionItem: function (key, value) {
        try {
            window.sessionStorage.setItem(key, value);
            return true;
        } catch {
            return false;
        }
    },
    removeSessionItem: function (key) {
        try {
            window.sessionStorage.removeItem(key);
            return true;
        } catch {
            return false;
        }
    },
    setTheme: function (theme) {
        try {
            document.documentElement.setAttribute('data-theme', theme === 'dark' ? 'dark' : 'light');
        } catch {
            // ignore
        }
    },
    downloadFile: function (filename, content, mimeType) {
        try {
            const blob = new Blob([content], { type: mimeType || 'text/plain;charset=utf-8' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = filename;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            setTimeout(() => URL.revokeObjectURL(url), 1000);
            return true;
        } catch {
            return false;
        }
    },
    focusElementById: function (id) {
        try {
            const el = document.getElementById(id);
            if (el) { el.focus(); el.select && el.select(); }
        } catch {
            // ignore
        }
    },
    printWindow: function () {
        window.print();
    }
};
