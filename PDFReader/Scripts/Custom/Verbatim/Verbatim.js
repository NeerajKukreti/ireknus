
$(document).ready(function () {

    $(document).on('click', '.keywords', function () {

        var foundkeywords = $(this).data('foundkeywords');
        var URL = $(this).data('url');

        $.ajax({
            url: SearchKeyword,
            type: 'GET',
            data: { Keyword: foundkeywords },
            beforeSend: function () { },
            success: function (data) {
                $('.keywordPhrases').empty();

                var cnt = 0;
                $.each(data, function (index, item) {

                    $.each(item.PhraseText, function (index1, item1) {

                        var htmlContent = `
                        <div class="col1">
                            <div class="cont">
                                <div data-pageno="${++cnt}" data-FoundKeywords="${item.FoundKeywords|| ''}"
                                     class="desc keywordPhrase" style="cursor: pointer;">
                                ${item.PDFPageNumber}: ${item1 || 'No keywords'}
                                </div>
                            </div>
                        </div>
                       `;

                        $('.keywordPhrases').append(htmlContent);

                    });
                });

            },
            error: function () { alert('error occured while trying to search') }
            ,
            complete: function () {
                $.ajax({
                    url: pdfText,
                    type: 'GET', 
                    beforeSend: function () { },
                    success: function (data) {
                        $('.pdfText').empty().html(data);
                    },
                    error: function () { alert('error occured while trying to search') }
                    ,
                    complete: function () {
                    }
                });
            }
        });
    });

    var orgText = $('.pdfText').html();

    $(document).on('click', '.keywordPhrase', function () {
        var key = $(this).data('foundkeywords');
        var index = $(this).data('pageno');
        var pdfText = $('.pdfText').html();


        // Escape special regex characters
         const regex = new RegExp(key.replace(/\s+/g, "\\s*(<[^>]+>\\s*)?"), "gi");

        

        let currentIndex = 0;
        const uniqueId = 'foundText-' + Date.now();

        // Replace only the specific occurrence
        const updatedText = pdfText.replace(regex, (match) => {
            currentIndex++;
            if (currentIndex === index) {
                return `<span id="${uniqueId}" class="found-text-highlight" style="color: red; background-color: rgba(255,255,0,0.3);">${match}</span>`;
            }
            return match; // Return original text for other occurrences
        });

        // Update the HTML only if the occurrence was found and replaced
        if (currentIndex >= index) {
            $('.pdfText').html(updatedText);

            // Scroll to the element
            setTimeout(function () {
                const targetElement = document.getElementById(uniqueId);
                if (targetElement) {
                    targetElement.scrollIntoView({
                        behavior: 'smooth',
                        block: 'center',
                        inline: 'nearest'
                    });
                }
            }, 50);
        } else {
            console.log('Occurrence not found');
        }




    });
});