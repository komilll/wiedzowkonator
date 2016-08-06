# wiedzowkonator
Wiedzówkonator to program służący do tworzenia i przeprowadzania quizów/wiedzówek. Program obsługuje pytania tekstowe, screenówkę, pytania muzyczne oraz wiedzówkę mieszaną, na którą składają się trzy poprzednie.

Importowanie plików:

-Pytania tekstowe (zapisane w formacie *.txt z formatowaniem UTF-8): Składnia pliku wygląda następująco. W linijkach nieparzystych (pierwsza, trzecia etc.) znajdują się tytuł umieszczony w cudzysłowiu "", a następnie w tej samej linijce znajduje się pytanie. Linijka nie musi zawierać tytułu ani cudzysłowiu, program poradzi sobie z jej interpretacją. Linijki parzyste zawierają odpowiedź na pytanie z wcześniejszej linijki.

-Screenówka: Program obsługuje większość formatów graficznych. Wystarczy wybrać obrazki, a następnie możemy wybrać jeden z trzech przedstawionych trybów odpowiedzi. Odpowiedzi na pytania mogą się nie wyświetlać, odpowiedzią będzie nazwa pliku graficznego lub możemy sami ustawić pytania dla obrazka, który nadpiszę jego nazwę. Od tej pory jeśli zaimportujemy ten sam obrazek będzie automatycznie posiadał ustawioną odpowiedź (można ją później zmienić).

-Pytania muzyczne: Program obsługuje większość plików muzycznych. Wystarczy wybrać pliki i zostaną one zaimportowane do prostego wewnętrznego playera (który wymaga nieco usprawnień).

Po każdorazowej odpowiedzi na pytanie i potwierdzeniu punktów stan wiedzówki jest zapisywany, aby w razie awarii wrócić do ostatniego stanu ("Załaduj ostatni szybki zapis..."). Save'y to pliki binarne o wadze ~2-3KB zapisywane w C:/Users/obecny_użytkownik/AppData/LocalLow/Wiedzowkonator/. Obecnie program niestety nie przewiduje systemu na partycji innej niż C:/. Istnieje możliwość ręcznego zapisywania stanu poprzez kliknięcie na górze "Plik --> Zapisz". Pozostałe przyciski to pozostałości z innych wersji programów: otwórz i wczytaj zostały zastąpione przez początkowe menu. "Usuń końcówki" służy do wykasowania dodatkowych danych zapisywanych przy screenach z własnymi odpowiedziami. Warto użyć jeśli chcemy ponownie użyć tych plików graficznych z oryginalną nazwą.

Program publikowany jest na licencji Creative Commons, więc jest praktycznie dobrem publicznym.
