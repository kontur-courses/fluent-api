# Fluent API

Даже если ты не слышал про Fluent API, то скорее всего применял LINQ, который является ярким представителем концепции.

Fluent API — это стиль оформления публичных интерфейсов. Использовать такой API удобно: автодополнение в IDE выдает релевантные подсказки, а получающийся код образует цельные фразы, которые легко читать.

А вот так выглядит код, в котором используется Fluent API:
``` cs
// Публичный интерфейс Fake It Easy реализован как Fluent API
A.CallTo(() => shop.GetTopSellingCandy()).Returns(lollipop);
```

Пройдя блок, ты:

- поймешь принципы Fluent API и сможешь более эффективно их использовать
- узнаешь об областях применения Fluent API
- научишься создавать собственные Fluent API


## Необходимые знания

Рекомендуется пройти блоки [LINQ](https://github.com/kontur-csharper/linq) и [Чистый код](https://github.com/kontur-csharper/clean-code)


## Самостоятельная подготовка

1. Посмотри [все видео-лекции про Fluent API](https://ulearn.azurewebsites.net/Course/cs2/Fluent_API_f317d52a-3a74-4138-98bf-565a5d593465) (~1 час)


## Очная встреча

~ 3 часа


## Закрепление материала

1. Спецзадание __Be fluent__  
Найди в своем проекте подзадачу, в которой Fluent API помог бы. Какие проблемы решит такой рефакторинг? 


## Дополнительные ссылки

* Курс на PluralSight [Designing Fluent APIs in C#](https://app.pluralsight.com/library/courses/designing-fluent-apis-c-sharp/table-of-contents)
