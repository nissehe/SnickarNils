window.blazorScroll = {
    save: function (key) {
        try {
            sessionStorage.setItem(key, window.scrollY.toString());
        } catch (e) {
            // ignore storage errors
        }
    },

    restore: function (key) {
        try {
            const v = sessionStorage.getItem(key);
            if (v !== null) {
                const y = parseInt(v, 10) || 0;

                const doScroll = function (pos) {
                    try {
                        // primary
                        window.scrollTo(0, pos);

                        // some browsers/layouts need explicit element writes
                        document.documentElement.scrollTop = pos;
                        document.body.scrollTop = pos;

                        // element-level scrollTo if supported
                        if (document.documentElement.scrollTo) {
                            document.documentElement.scrollTo(0, pos);
                        }
                        if (document.body.scrollTo) {
                            document.body.scrollTo(0, pos);
                        }
                    } catch (err) {
                        console.log("scroll error", err);
                    }
                };

                // Try immediate
                doScroll(y);

                // Retry on next animation frames (gives browser a chance to layout images/CSS)
                requestAnimationFrame(function () {
                    doScroll(y);
                    requestAnimationFrame(function () {
                        doScroll(y);
                    });
                });

                // Fallback after a short delay in case images or fonts change layout
                setTimeout(function () {
                    doScroll(y);
                }, 150);

                // Fallback after a longer delay in case images or fonts change layout
                setTimeout(function () {
                    doScroll(y);
                }, 1000);
            }
        } catch (e) {
        }
    },

    clear: function (key) {
        try {
            sessionStorage.removeItem(key);
        } catch (e) { }
    }
};