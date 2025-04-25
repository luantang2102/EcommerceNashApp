(function ($) {

    "use strict";

    // init jarallax parallax
    var initJarallax = function () {
        jarallax(document.querySelectorAll(".jarallax"));

        jarallax(document.querySelectorAll(".jarallax-img"), {
            keepImg: true,
        });
    }

    // input spinner
    var initProductQty = function () {

        $('.product-qty').each(function () {

            var $el_product = $(this);
            var quantity = 0;

            $el_product.find('.quantity-right-plus').click(function (e) {
                e.preventDefault();
                var quantity = parseInt($el_product.find('.quantity').val());
                $el_product.find('.quantity').val(quantity + 1);
            });

            $el_product.find('.quantity-left-minus').click(function (e) {
                e.preventDefault();
                var quantity = parseInt($el_product.find('.quantity').val());
                if (quantity > 0) {
                    $el_product.find('.quantity').val(quantity - 1);
                }
            });

        });

    }

    // init Chocolat light box
    var initChocolat = function () {
        Chocolat(document.querySelectorAll('.image-link'), {
            imageSize: 'contain',
            loop: true,
        })
    }

    // Animate Texts
    var initTextFx = function () {
        $('.txt-fx').each(function () {
            var newstr = '';
            var count = 0;
            var delay = 0;
            var stagger = 10;
            var words = this.textContent.split(/\s/);

            $.each(words, function (key, value) {
                newstr += '<span class="word">';

                for (var i = 0, l = value.length; i < l; i++) {
                    newstr += "<span class='letter' style='transition-delay:" + (delay + stagger * count) + "ms;'>" + value[i] + "</span>";
                    count++;
                }
                newstr += '</span>';
                newstr += "<span class='letter' style='transition-delay:" + delay + "ms;'> </span>";
                count++;
            });

            this.innerHTML = newstr;
        });
    }

    $(document).ready(function () {

        initProductQty();
        initJarallax();
        initChocolat();
        initTextFx();

        $(".user-items .search-item").click(function () {
            $(".search-box").toggleClass('active');
            $(".search-box .search-input").focus();
        });
        $(".close-button").click(function () {
            $(".search-box").toggleClass('active');
        });

        var breakpoint = window.matchMedia('(max-width:61.93rem)');

        if (breakpoint.matches === false) {

            var swiper = new Swiper(".main-swiper", {
                slidesPerView: 1,
                spaceBetween: 48,
                loop: true,
                autoplay: {
                    delay: 3000,
                    disableOnInteraction: false,
                },
                pagination: {
                    el: ".swiper-pagination",
                    clickable: true,
                },
                breakpoints: {
                    900: {
                        slidesPerView: 2,
                        spaceBetween: 48,
                    },
                },
            });

            // homepage 2 slider
            var swiper = new Swiper(".thumb-swiper", {
                direction: 'horizontal',
                slidesPerView: 6,
                spaceBetween: 6,
                breakpoints: {
                    900: {
                        direction: 'vertical',
                        spaceBetween: 6,
                    },
                },
            });
            var swiper2 = new Swiper(".large-swiper", {
                spaceBetween: 48,
                effect: 'fade',
                slidesPerView: 1,
                pagination: {
                    el: ".swiper-pagination",
                    clickable: true,
                },
                thumbs: {
                    swiper: swiper,
                },
            });

        }

        // product single page
        var thumb_slider = new Swiper(".product-thumbnail-slider", {
            slidesPerView: 5,
            spaceBetween: 10,
            // autoplay: true,
            direction: "vertical",
            breakpoints: {
                0: {
                    direction: "horizontal"
                },
                992: {
                    direction: "vertical"
                },
            },
        });

        // product large
        var large_slider = new Swiper(".product-large-slider", {
            slidesPerView: 1,
            // autoplay: true,
            spaceBetween: 0,
            effect: 'fade',
            thumbs: {
                swiper: thumb_slider,
            },
            pagination: {
                el: ".swiper-pagination",
                clickable: true,
            },
        });

        // Handle nested dropdowns on hover (desktop)
        $('.dropdown-submenu').hover(
            function () {
                if ($(window).width() > 991.98) { // Only on desktop
                    $(this).find('> .dropdown-menu').addClass('show');
                }
            },
            function () {
                if ($(window).width() > 991.98) { // Only on desktop
                    $(this).find('> .dropdown-menu').removeClass('show');
                }
            }
        );

        // Handle nested dropdowns on click (mobile and desktop fallback)
        $('.dropdown-submenu > a').on('click', function (e) {
            var submenu = $(this).next('.dropdown-menu');
            $('.dropdown-menu').not(submenu).removeClass('show'); // Close other submenus
            submenu.toggleClass('show'); // Toggle the current submenu
            e.stopPropagation(); // Prevent closing parent dropdowns
            e.preventDefault(); // Prevent navigation
        });

        // Close nested dropdowns when clicking outside
        $(document).on('click', function (e) {
            if (!$(e.target).closest('.dropdown-submenu').length) {
                $('.dropdown-submenu .dropdown-menu').removeClass('show');
            }
        });

        // Prevent Bootstrap's default dropdown behavior for nested dropdowns
        $('.dropdown-submenu .dropdown-toggle').on('click', function (e) {
            e.stopPropagation(); // Prevent Bootstrap from closing the parent dropdown
        });

    }); // End of a document

    $(window).load(function () {
        $('.preloader').fadeOut();
    });

})(jQuery);