(function () {
    var dict = {
        Turkish: {
            downloads: 'İndirmeler',
            completed: 'Tamamlanan',
            settings: 'Ayarlar',
            completedDownloads: 'Tamamlanan İndirmeler',
            clearAll: 'Tümünü Temizle',
            loading: 'Yükleniyor...',
            noCompleted: 'Henüz tamamlanan indirme yok',
            itemsSuffix: 'öğe',
            clearConfirm: 'Tüm indirme geçmişi temizlensin mi?',
            statusCompleted: 'TAMAMLANDI',
            statusFailed: 'BAŞARISIZ',
            newDownload: 'Yeni İndirme',
            videoUrl: 'Video URL',
            pastePlaceholder: 'YouTube, Vimeo vb. URL yapıştırın...',
            analyze: 'Analiz Et',
            quality: 'Kalite',
            videoQuality: 'Video Kalitesi',
            audioQuality: 'Ses Kalitesi',
            formatOptions: 'Format Seçenekleri',
            container: 'Kapsayıcı',
            mp4: 'MP4 (Yaygın uyumluluk)',
            mkv: 'MKV (Gelişmiş özellikler)',
            webm: 'WebM (Web için optimize)',
            mp3: 'MP3 (Yalnızca ses)',
            subtitles: 'Altyazıları indir',
            extractAudio: 'Ses parçasını çıkar',
            best: 'En iyi kalite',
            auto: 'Otomatik',
            ultra: '4K Ultra HD',
            recommended: 'Önerilen',
            start: 'İndirmeyi başlat',
            active: 'Aktif',
            noActive: 'Aktif indirme yok',
            supported: 'Desteklenen Siteler',
            about: 'Hakkında',
            aboutTitle: 'Video Downloader Hakkında',
            aboutDesc: 'YouTube, Twitter, Instagram, TikTok, Facebook, Twitch, Kick ve 50’den fazla platformdan video indirin.',
            technologies: 'Teknolojiler:',
            developer: 'Geliştirici:',
            close: 'Kapat'
        },
        English: {
            downloads: 'Downloads',
            completed: 'Completed',
            settings: 'Settings',
            completedDownloads: 'Completed Downloads',
            clearAll: 'Clear All',
            loading: 'Loading...',
            noCompleted: 'No completed downloads yet',
            itemsSuffix: 'items',
            clearConfirm: 'Clear all history?',
            statusCompleted: 'COMPLETED',
            statusFailed: 'FAILED',
            newDownload: 'New Download',
            videoUrl: 'Video URL',
            pastePlaceholder: 'Paste URL from YouTube, Vimeo, etc...',
            analyze: 'Analyze',
            quality: 'Quality',
            videoQuality: 'Video Quality',
            audioQuality: 'Audio Quality',
            formatOptions: 'Format Options',
            container: 'Container',
            mp4: 'MP4 (Widely Compatible)',
            mkv: 'MKV (Advanced Features)',
            webm: 'WebM (Optimized for Web)',
            mp3: 'MP3 (Audio Only)',
            subtitles: 'Download Subtitles',
            extractAudio: 'Extract Audio Track',
            best: 'Best Quality',
            auto: 'Auto',
            ultra: '4K Ultra HD',
            recommended: 'Recommended',
            start: 'Start Download',
            active: 'Active',
            noActive: 'No active downloads',
            supported: 'Supported Sites',
            about: 'About',
            aboutTitle: 'About Video Downloader',
            aboutDesc: 'Download videos from YouTube, Twitter, Instagram, TikTok, Facebook, Twitch, Kick and 50+ other platforms.',
            technologies: 'Technologies:',
            developer: 'Developer:',
            close: 'Close'
        }
    };

    function getLang() {
        var l = localStorage.getItem('vdlang');
        if (l) {
            l = l.replace(/["'\\]/g, '').trim();
            if (dict[l]) return l;
        }
        return (navigator.language && navigator.language.startsWith('tr')) ? 'Turkish' : 'English';
    }

    function t(k) {
        var l = getLang();
        var d = dict[l] || dict.English;
        return (d && d[k] !== undefined) ? d[k] : (dict.English[k] || k);
    }
    window.t = t;
    window.vdGetLang = getLang;

    function setText(selector, value) {
        var el = document.querySelector(selector);
        if (el) el.textContent = value;
    }

    function setLabel(selector, value) {
        var el = document.querySelector(selector);
        if (!el) return;
        for (var i = el.childNodes.length - 1; i >= 0; i--) {
            if (el.childNodes[i].nodeType === 3) {
                el.childNodes[i].nodeValue = value;
                return;
            }
        }
        el.appendChild(document.createTextNode(value));
    }

    function makeDeveloperLink() {
        var footerBrand = document.querySelector('footer > div:first-child');
        if (footerBrand && footerBrand.tagName !== 'A') {
            var link = document.createElement('a');
            link.href = 'https://kayapater.dev';
            link.target = '_blank';
            link.rel = 'noopener noreferrer';
            link.className = footerBrand.className + ' hover:text-primary transition-colors';
            link.textContent = 'kayapater';
            footerBrand.replaceWith(link);
        }
    }

    function applyCommon() {
        var lang = getLang();
        var turkish = (lang === 'Turkish');
        document.documentElement.lang = turkish ? 'tr' : 'en';
        window.vdLanguage = lang;

        // Sidebar Navigation
        setLabel('aside a[href*="download"]', t('downloads'));
        setLabel('aside a[href*="history"]', t('completed'));
        setLabel('aside a[href*="settings"]', t('settings'));

        // Footer & Modals
        var aboutLink = document.querySelector('footer a[onclick*="showAbout"]');
        if (aboutLink) aboutLink.textContent = t('about');
        var supportedLink = document.querySelector('footer a[onclick*="showSupported"]');
        if (supportedLink) supportedLink.textContent = t('supported');

        setText('#aboutModal h3', t('aboutTitle'));
        setText('#aboutModal p', t('aboutDesc'));
        setText('#aboutModal strong[data-i18n="technologies"]', t('technologies'));
        setText('#aboutModal strong[data-i18n="developer"]', t('developer'));
        document.querySelectorAll('#aboutModal button').forEach(function(b) { b.textContent = t('close'); });
        setText('#sitesModal h3', t('supported'));
        document.querySelectorAll('#sitesModal button').forEach(function(b) { b.textContent = t('close'); });

        // History Page
        var isHistory = location.pathname.toLowerCase().indexOf('history.html') >= 0;
        if (isHistory) {
            setText('main h2', t('completedDownloads'));
            var clearBtn = document.querySelector('main button[onclick="clearHistory()"]');
            if (clearBtn) {
                for (var i = clearBtn.childNodes.length - 1; i >= 0; i--) {
                    if (clearBtn.childNodes[i].nodeType === 3) {
                        clearBtn.childNodes[i].nodeValue = t('clearAll');
                        break;
                    }
                }
            }
            var loadingP = document.querySelector('#historyList > p');
            if (loadingP && (loadingP.textContent.trim().indexOf('Loading') === 0 || loadingP.textContent.trim().indexOf('Yükleniyor') === 0)) {
                loadingP.textContent = t('loading');
            }
        }

        // Download Page
        var isDownload = location.pathname.toLowerCase().indexOf('download.html') >= 0 || location.pathname === '/' || location.pathname.endsWith('/');
        if (isDownload) {
            setText('main header h1', t('newDownload'));
            setText('label[for="urlInput"], label', t('videoUrl'));
            var input = document.getElementById('urlInput');
            if (input) input.placeholder = t('pastePlaceholder');
            var analyze = document.querySelector('button[onclick="analyzeUrl()"]');
            if (analyze && analyze.lastChild) analyze.lastChild.textContent = t('analyze');

            var h3s = document.querySelectorAll('main h3');
            if (h3s.length >= 2) {
                var f = document.getElementById('formatSelect');
                var isAudio = f && f.value.indexOf('MP3') === 0;
                h3s[0].textContent = isAudio ? t('audioQuality') : t('videoQuality');
                h3s[1].textContent = t('formatOptions');
            }

            setText('#formatSelect option:nth-child(1)', t('mp4'));
            setText('#formatSelect option:nth-child(2)', t('mkv'));
            setText('#formatSelect option:nth-child(3)', t('webm'));
            setText('#formatSelect option:nth-child(4)', t('mp3'));
            setText('label[for="subCheck"] span, #subCheck + span', t('subtitles'));
            setText('label[for="audioCheck"] span, #audioCheck + span', t('extractAudio'));

            var noAct = document.querySelector('#activeList > p');
            if (noAct && (noAct.textContent.indexOf('No active') >= 0 || noAct.textContent.indexOf('Aktif indirme yok') >= 0)) {
                noAct.textContent = t('noActive');
            }
            document.querySelectorAll('button').forEach(function (button) {
                if (button.textContent.indexOf('Start Download') >= 0 || button.textContent.indexOf('İndirmeyi başlat') >= 0) {
                    if (button.lastChild) button.lastChild.textContent = t('start');
                }
            });
        }

        makeDeveloperLink();
    }

    window.vdApplyLanguage = applyCommon;

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', applyCommon);
    } else {
        applyCommon();
    }
})();
